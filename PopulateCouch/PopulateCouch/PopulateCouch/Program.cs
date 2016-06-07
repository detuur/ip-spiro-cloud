using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Couchbase;
using Couchbase.Configuration.Client;

namespace PopulateCouch
{
#region helper classes
    class Measurement
    {
        public string type = "measurement";
        public DateTime dateTime;
        public string id;
        public string userID;
        public string rawDataID;
        public int FVC;
        public int FEV1;
        public int FEV3;
        public int FEV6;
        public int PEF;
        public int FEF;
        public int QualityFactor;
    }
    class RawData
    {
        public string type = "rawdata";
        public string id;
        public string dataID;
        public int[] data;
    }
    class Patient
    {
        public string type = "user";
        public string id;
        public string first_name;
        public string last_name = "Anonymous";
        public string birthdate;
        public string sex;
        public string race;
        public float height;
        public float weight;
    }
#endregion
    class Program
    {
        static HashSet<string> users = new HashSet<string>();
        static List<Patient> patients = new List<Patient>();
        static List<Measurement> metings = new List<Measurement>();
        static List<RawData> rawdatas = new List<RawData>();
        static Random rand = new Random();

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: PopulateCouch.exe NH3SPIRO.CSV adult.dat youth.dat exam.dat\n" +
                                  "Where every file can be a path.");
                return;
            }
            getPatients(args[0], args[1], args[2], args[3]);
            Upload();
        }

        static void getPatients(string spiroDataPath, string adultDataPath, string youthDataPath, string examDataPath)
        {
            Console.WriteLine("Processing Spiro data...");
            int counter = 0;
            using (var reader = new StreamReader(File.OpenRead(spiroDataPath)))
            {
                while (!reader.EndOfStream)
                {
                    var values = reader.ReadLine().Split(',');
                    var id = values[0];
                    if (int.Parse(id) % 500 == 0)
                    {
                        Console.Clear();
                        Console.WriteLine(id);
                    }
                    var met = new Measurement()
                    {
                        userID = "user-NHANES3-" + id.PadLeft(5, '0'),
                        FVC = Convert.ToInt32(values[9]),
                        FEV1 = Convert.ToInt32(values[10]),
                        FEV3 = Convert.ToInt32(values[11]),
                        FEV6 = Convert.ToInt32(values[12]),
                        PEF = Convert.ToInt32(values[13]),
                        FEF = Convert.ToInt32(values[14]),
                        dateTime = new DateTime(1, 1, 1, Convert.ToInt32(values[2]), Convert.ToInt32(values[3]), 0),
                        QualityFactor = Convert.ToInt32(values[4])
                    };
                    var TechnicianQualityFactor = Convert.ToInt32(values[5]);
                    var QFba = new BitArray(new int[] { met.QualityFactor });
                    if (!QFba[0] && !QFba[2])
                    {
                        int[] flowarr = Array.ConvertAll<string, int>(values.Skip(17).ToArray(), int.Parse);
                        if (flowarr.Count() > 1525)
                        {
                            var substack = new Stack<int>(flowarr.Skip(1525).Reverse());
                            Stack<int> flowstack = new Stack<int>(flowarr.Take(1525));
                            while (substack.Count != 0)
                            {
                                flowstack.Push((flowstack.Peek() + substack.Peek()) / 2);
                                flowstack.Push(substack.Pop());
                            }
                            flowarr = flowstack.Reverse().ToArray();
                        }
                        var raw = new RawData()
                        {
                            dataID = (++counter).ToString(),
                            data = flowarr
                        };
                        if (!users.Contains(id))
                        {
                            patients.Add(new Patient() { id = id });
                            users.Add(id);
                        }
                        if (Convert.ToInt32(values[6]) == 0)
                        {
                            met.rawDataID = counter.ToString();
                            metings.Add(met);
                            rawdatas.Add(raw);
                        }
                        else
                        {
                            metings.Add(met);
                        }
                    }
                }
            }
            Console.WriteLine("Processing exam data...");
            using (var reader = new StreamReader(File.OpenRead(examDataPath)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var id = line.Substring(0, 5).TrimStart('0');
                    if (users.Contains(id))
                    {
                        var pat = patients.Find(p => p.id == id);
                        pat.weight = Convert.ToSingle(line.Substring(1507, 6));
                        try { pat.height = Convert.ToSingle(line.Substring(1527, 5)); }
                        catch (Exception ex) { pat.height = Convert.ToSingle(line.Substring(1538, 5)); }
                    }
                }
            }
            Console.WriteLine("Processing adult data...");
            using (var reader = new StreamReader(File.OpenRead(adultDataPath)))
            {
                while (!reader.EndOfStream)
                {
                    lineparse(reader.ReadLine());
                }
            }
            Console.WriteLine("Processing youth data...");
            using (var reader = new StreamReader(File.OpenRead(youthDataPath)))
            {
                while (!reader.EndOfStream)
                {
                    lineparse(reader.ReadLine(), true);
                }
            }
        }
        static void lineparse(string line, bool yth = false)
        {
            var id = line.Substring(0, 5).TrimStart('0');
            if (users.Contains(id))
            {
                users.Remove(id);
                if (id.TrimEnd('0') != id)
                {
                    Console.Clear();
                    Console.WriteLine(id);
                }
                var pat = patients.Find(p => p.id == id);
                pat.id = "user-NHANES3-" + id.PadLeft(5, '0');
                var met = metings.Where(m => m.userID == pat.id);
                pat.first_name = "Subject" + id.PadLeft(5, '0');
                pat.race = ((Race)Convert.ToInt32(line.Substring(11, 1))).ToString();
                pat.sex = ((Sex)Convert.ToInt32(line.Substring(14, 1))).ToString();
                int agemonth = Convert.ToInt32(line.Substring(20, 4));

                int day, month;
                string sday = line.Substring(yth ? 1159 : 1235, 1);
                string smonth = line.Substring(yth ? 1157 : 1233, 2);
                string sdayHome = line.Substring(yth ? 1166 : 1243, 1);
                string smonthHome = line.Substring(yth ? 1164 : 1241, 2);
                if (sday == " " && smonth == "  ")
                {
                    day = Convert.ToInt32(sdayHome);
                    month = Convert.ToInt32(smonthHome);
                }
                else if (sdayHome == " " && smonthHome == "  ")
                {
                    day = Convert.ToInt32(sday);
                    month = Convert.ToInt32(smonth);
                }
                else
                    throw new Exception("no!");

                var date = new DateTime(1990, month, day);
                var age = new TimeSpan((int)Math.Round((double)agemonth * 30.5, MidpointRounding.AwayFromZero), 0, 0, 0);
                pat.birthdate = (date - age).ToShortDateString();
                foreach (var m in met)
                {
                    var dt = date + new TimeSpan(m.dateTime.Hour, m.dateTime.Minute, 0);
                    while (met.Count(mm => mm.dateTime == dt) > 0)
                        dt = dt + TimeSpan.FromSeconds(10);
                    m.dateTime = dt;
                    m.id = "data-NHANES3-" + id.PadLeft(5, '0') + "-" + dt.ToString("yyyyMMddTHHmmss");
                    if (rawdatas.Exists(d => d.dataID == m.rawDataID))
                    {
                        var data = rawdatas.First(d => d.dataID == m.rawDataID);
                        data.id = "raw" + m.id.Substring(4);
                        data.dataID = m.id;
                        m.rawDataID = data.id;
                    }
                    else
                    {
                        m.rawDataID = null;
                    }
                }
            }
        }

        enum Sex { male = 1, female = 2 }
        enum Race { white = 1, black = 2, latino = 3, other = 4 }

        static void Upload()
        {
            Console.WriteLine("Connecting to bucket...");
            var config = new ClientConfiguration
            {
                Servers = new List<Uri>
        {
            new Uri("http://127.0.0.1:8091/pools")
        }
            };
            Cluster Cluster = new Cluster(config);

            using (var bucket = Cluster.OpenBucket("default"))
            {
                var manager = bucket.CreateManager("Administrator", "spirometer");
                manager.Flush();

                Console.WriteLine("Uploading patient data...");
                foreach (var pat in patients)
                {
                    var doc = new Document<Patient>
                    {
                        Id = pat.id,
                        Content = pat
                    };

                    var succ = bucket.Upsert(doc);
                    if (!succ.Success)
                        throw new Exception("FUCK");
                }
                Console.WriteLine("Uploading spiro data...");
                foreach (var met in metings)
                {
                    var doc = new Document<Measurement>
                    {
                        Id = met.id,
                        Content = met
                    };

                    var succ = bucket.Upsert(doc);
                    if (!succ.Success)
                        throw new Exception("FUCKK");
                }
                Console.WriteLine("Uploading raw data...");
                foreach (var raw in rawdatas)
                {
                    var doc = new Document<RawData>
                    {
                        Id = raw.id,
                        Content = raw
                    };

                    var succ = bucket.Upsert(doc);
                    if (!succ.Success)
                        throw new Exception("FUCKKK");
                }
                Console.WriteLine("Done.");
            }
        }
    }
}
