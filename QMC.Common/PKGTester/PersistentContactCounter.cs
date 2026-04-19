using System;
using System.IO;
using Newtonsoft.Json;   // 추가

namespace QMC.LCP_280.Process.Component
{
    public sealed class PersistentContactCounter
    {
        private readonly object _sync = new object();
        private readonly string _filePath;

        public long TotalCount { get; private set; }

        private class CounterDto
        {
            public long TotalCount { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        public PersistentContactCounter(string filePath = null)
        {
            _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "contact_counter.json");
            Load();
        }

        public long Increase(long delta = 1)
        {
            lock (_sync)
            {
                if (delta < 0) delta = 0;
                TotalCount += delta;
                Save();
                return TotalCount;
            }
        }

        public void Reset(long value = 0)
        {
            lock (_sync)
            {
                TotalCount = Math.Max(0, value);
                Save();
            }
        }

        public void Load()
        {
            lock (_sync)
            {
                try
                {
                    if (!File.Exists(_filePath))
                    {
                        TotalCount = 0;
                        Save();
                        return;
                    }

                    var json = File.ReadAllText(_filePath);
                    var dto = JsonConvert.DeserializeObject<CounterDto>(json); // 변경
                    TotalCount = dto?.TotalCount ?? 0;
                }
                catch
                {
                    TotalCount = 0;
                    Save();
                }
            }
        }

        public void Save()
        {
            lock (_sync)
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var dto = new CounterDto
                {
                    TotalCount = TotalCount,
                    LastUpdated = DateTime.Now
                };

                var json = JsonConvert.SerializeObject(dto, Formatting.Indented); // 변경
                File.WriteAllText(_filePath, json);
            }
        }
    }
}