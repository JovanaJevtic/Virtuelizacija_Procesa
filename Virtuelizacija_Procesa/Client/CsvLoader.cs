using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class CsvLoader
    {

        public List<SensorSample> LoadCsv(out List<string> invalidRows, int maxRows = 100)
        {
            invalidRows = new List<string>();
            List<SensorSample> samples = new List<SensorSample>();

            string csvPath = ConfigurationManager.AppSettings["CsvPath"];

            if (!File.Exists(csvPath))
                throw new FileNotFoundException($"CSV file not found at {csvPath}");

            using (var reader = new StreamReader(csvPath))
            {
                int lineNumber = 0;

                string header = reader.ReadLine();

                while (!reader.EndOfStream && lineNumber < maxRows)
                {
                    string line = reader.ReadLine();
                    lineNumber++;

                    var fields = line.Split(',');

                    try
                    {
                        if (fields.Length < 6)
                            throw new Exception("Invalid number of columns");

                        SensorSample sample = new SensorSample
                        {
                            DateTime = DateTime.Parse(fields[0], CultureInfo.InvariantCulture),
                            Volume = double.Parse(fields[1], CultureInfo.InvariantCulture),
                            T_DHT = double.Parse(fields[3], CultureInfo.InvariantCulture),
                            Pressure = double.Parse(fields[4], CultureInfo.InvariantCulture),
                            T_BMP = double.Parse(fields[5], CultureInfo.InvariantCulture),
                         
                        };

                        samples.Add(sample);
                    }
                    catch
                    {
                        invalidRows.Add(line);
                    }
                }
            }

            if (invalidRows.Count > 0)
            {
                string logPath = ConfigurationManager.AppSettings["LogPath"];
                using (StreamWriter logWriter = new StreamWriter(logPath, true))
                {
                    foreach (var invalid in invalidRows)
                        logWriter.WriteLine(invalid);
                }
            }

            Console.WriteLine($"Ukupno uspešno učitanih redova: {samples.Count}");

            return samples;
        }

    }
}
