using System.Collections.Concurrent;
using Tesseract;

namespace Makeos.Utilities
{
    public interface ITesseractEnginePool
    {
        /// <summary>
        /// Toma prestado un motor para el idioma indicado. Liberar el objeto devuelto lo
        /// reintegra al pool para su reutilización.
        /// </summary>
        PooledEngine Rent(string languages);
    }

    /// <summary>
    /// Pool de motores Tesseract reutilizables. Crear un motor es costoso y, además,
    /// <see cref="TesseractEngine"/> no es thread-safe: cada hilo debe usar su propio motor.
    /// El pool mantiene motores ociosos por idioma y los reutiliza entre peticiones.
    /// </summary>
    public sealed class TesseractEnginePool : ITesseractEnginePool, IDisposable
    {
        private static readonly string TessDataPath =
            Path.Combine(AppContext.BaseDirectory, "Data", "tessdata");

        private readonly int _maxIdlePerLanguage;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<TesseractEngine>> _idle = new();
        private volatile bool _disposed;

        public TesseractEnginePool(IConfiguration configuration)
        {
            // 0 o ausente => valor por defecto basado en los núcleos disponibles.
            var configured = configuration.GetValue<int?>("Ocr:MaxPoolSize") ?? 0;
            _maxIdlePerLanguage = configured > 0 ? configured : Environment.ProcessorCount;
        }

        public PooledEngine Rent(string languages)
        {
            var queue = _idle.GetOrAdd(languages, _ => new ConcurrentQueue<TesseractEngine>());
            if (!queue.TryDequeue(out var engine))
            {
                engine = new TesseractEngine(TessDataPath, languages, EngineMode.Default);
            }

            return new PooledEngine(this, languages, engine);
        }

        internal void Return(string languages, TesseractEngine engine)
        {
            var queue = _idle.GetOrAdd(languages, _ => new ConcurrentQueue<TesseractEngine>());

            // Si ya hay suficientes motores ociosos (o el pool se está desechando), se libera
            // el motor en lugar de retenerlo, para acotar el uso de memoria nativa.
            if (_disposed || queue.Count >= _maxIdlePerLanguage)
            {
                engine.Dispose();
                return;
            }

            queue.Enqueue(engine);
        }

        public void Dispose()
        {
            _disposed = true;
            foreach (var queue in _idle.Values)
            {
                while (queue.TryDequeue(out var engine))
                {
                    engine.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Motor tomado del pool. Al liberarlo (Dispose) se devuelve al pool en vez de destruirse.
    /// </summary>
    public sealed class PooledEngine : IDisposable
    {
        private readonly TesseractEnginePool _pool;
        private readonly string _languages;
        private bool _returned;

        internal PooledEngine(TesseractEnginePool pool, string languages, TesseractEngine engine)
        {
            _pool = pool;
            _languages = languages;
            Engine = engine;
        }

        public TesseractEngine Engine { get; }

        public void Dispose()
        {
            if (_returned)
            {
                return;
            }

            _returned = true;
            _pool.Return(_languages, Engine);
        }
    }
}
