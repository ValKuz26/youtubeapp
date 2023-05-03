using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Data.SQLite;
using System.IO;

namespace YouTubeVideoInfo
{
    internal class Program
    {
        public class Default
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class High
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Item
        {
            public string kind { get; set; }
            public string etag { get; set; }
            public string id { get; set; }
            public Snippet snippet { get; set; }
        }

        public class Localized
        {
            public string title { get; set; }
            public string description { get; set; }
        }

        public class Maxres
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Medium
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class PageInfo
        {
            public int totalResults { get; set; }
            public int resultsPerPage { get; set; }
        }

        public class Root
        {
            //корінь - root
            public string kind { get; set; }
            public string etag { get; set; }
            public List<Item> items { get; set; }
            public PageInfo pageInfo { get; set; }
        }

        public class Snippet
        {
            public DateTime publishedAt { get; set; }
            public string channelId { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public Thumbnails thumbnails { get; set; }
            public string channelTitle { get; set; }
            public List<string> tags { get; set; }
            public string categoryId { get; set; }
            public string liveBroadcastContent { get; set; }
            public Localized localized { get; set; }
            public string defaultAudioLanguage { get; set; }
        }

        public class Standard
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Thumbnails
        {
            public Default @default { get; set; }
            public Medium medium { get; set; }
            public High high { get; set; }
            public Standard standard { get; set; }
            public Maxres maxres { get; set; }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Enter video ID:");
            string[] data = Console.ReadLine().Trim().Split();
            string videoid = data[0];

            GetVideoInfo(videoid);
            Console.WriteLine("Enter video title:");
            string[] data1 = Console.ReadLine().Trim().Split();
            string name = data1[0];
            ReadDataBase(name);
        }
        public static void GetVideoInfo(string videoid)
        {
            HttpClient client = new HttpClient();
            string apiKey = "AIzaSyCaL0hANB62sSpivBzJKaLztACUivZPpNQ";
            string url = "https://youtube.googleapis.com/youtube/v3/videos?part=snippet&id=" + videoid + "&key=" + apiKey;
            //відповідь
            Console.WriteLine(url);

            string response = "";
            response = client.GetStringAsync(url).Result.ToString();
            Root videoInfo = JsonSerializer.Deserialize<Root>(response);
            Item video = videoInfo.items[0];
            Console.WriteLine(video.id + " " + video.snippet.title + " " + video.snippet.publishedAt);
            SaveToDB(video);
        }
        static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return sqlite_conn;
        }
        public static void SaveToDB(Item video)
        {
            SQLiteConnection sqlite_connection;
            sqlite_connection = CreateConnection();
            CreateTable(sqlite_connection);
            InsertData(sqlite_connection, video);
        }
        static void CreateTable(SQLiteConnection connection)
        {
            SQLiteCommand sqlite_cmd;
            string Createsql = "CREATE TABLE IF NOT EXISTS Videos (id VARCHAR(20) PRIMARY KEY ON CONFLICT REPLACE, title VARCHAR(50), publishedAt VARCHAR(20))";
            sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
        }
        static void InsertData(SQLiteConnection connection, Item video)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = $"INSERT INTO Videos (id, title, publishedAt)  VALUES('{video.id}', '{video.snippet.title.Replace('\'', '`')}', '{video.snippet.publishedAt}');";
            //заповнюємо таблицю в базі даних
            sqlite_cmd.ExecuteNonQuery();
        }
        static void ReadDataBase(string name)
        {
            SQLiteConnection connection;
            connection = CreateConnection();
            //зчитує з бази данних інфу
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = connection.CreateCommand();
            //sqlite_cmd.CommandText = "SELECT * FROM Videos";
            sqlite_cmd.CommandText = $"SELECT * FROM Videos WHERE title like '%{name}%'";
            sqlite_datareader = sqlite_cmd.ExecuteReader();


            while (sqlite_datareader.Read())
            {
                string id = sqlite_datareader.GetString(0);
                string title = sqlite_datareader.GetString(1);
                string publishedAt = sqlite_datareader.GetString(2);
                string nameString = id + " " + title + " " + publishedAt;
                //strings.Add(nameString);
                Console.WriteLine(nameString);//виводит JSON в консоль
            }
            //connection.Close();
            WriteToFile(connection, sqlite_datareader, sqlite_cmd);
        }
        static void WriteToFile(SQLiteConnection connection, SQLiteDataReader sqlite_datareader, SQLiteCommand sqlite_cmd)
        {
            sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Videos";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            string filePath = "videosDataBase.txt";
            List<string> strings = new List<string>();

            while (sqlite_datareader.Read())
            {
                string id = sqlite_datareader.GetString(0);
                string title = sqlite_datareader.GetString(1);
                string publishedAt = sqlite_datareader.GetString(2);
                string nameString = id + " " + title + " " + publishedAt;
                strings.Add(title);
            }
            using (FileStream filestream = File.Open(filePath, FileMode.Create))
            {
                using (StreamWriter output = new StreamWriter(filestream))
                {
                    foreach (string aString in strings)
                    {
                        output.WriteLine(aString);
                    }
                }
            }
            connection.Close();
            //comment
        }
    }
}
