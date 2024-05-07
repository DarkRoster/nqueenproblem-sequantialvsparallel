using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace N_Queen_Problem
{
    internal class Program
    {
        int N; // Boyutu tahta
        int numQueens; // Vezir sayısı
        int totalSolutions; // Toplam çözüm sayısı
        object lockObject = new object(); // Çözüm sayısını güvenli bir şekilde artırmak için kilit nesnesi

        // Constructor
        public Program(int n, int queens)
        {
            N = n;
            numQueens = queens;
            totalSolutions = 0;
        }

        // create A list of sequential solutions for time and count
        static List<string> saveSolutionCountAndTime = new List<string>();
        static List<string> saveSolutionCountAndTimeSequential = new List<string>();

        // Save solution count and time to list
        static void SaveSolutionCountAndTime(int queens, int totalSolutions, long time)
        {
            // save data to saveSolutionCountAndTime
            saveSolutionCountAndTime.Add($"{queens}, {totalSolutions}, {time}");
        }

        static void SaveSolutionCountAndTimeSequential(int queens, int totalSolutions, long time)
        {
            // save data to saveSolutionCountAndTimeSequential
            saveSolutionCountAndTimeSequential.Add($"{queens}, {totalSolutions / 2}, {time}");
        }

        // let save data to csv file
        static void SaveDataToCsvFile()
        {
            string directoryPath = @"C:\Users\Furkan\Desktop\csv\";

            // save saveSolutionCountAndTime to csv file
            using (StreamWriter file = new StreamWriter(Path.Combine(directoryPath, "saveSolutionCountAndTime.csv")))
            {
                file.WriteLine("Vezir Sayısı, Çözüm Sayısı, Geçen Süre (ms)");
                foreach (var item in saveSolutionCountAndTime)
                {
                    file.WriteLine(item);
                }
            }

            // save saveSolutionCountAndTimeSequential to csv file
            using (StreamWriter file = new StreamWriter(Path.Combine(directoryPath, "saveSolutionCountAndTimeSequential.csv")))
            {
                file.WriteLine("Vezir Sayısı, Çözüm Sayısı, Geçen Süre (ms)");
                foreach (var item in saveSolutionCountAndTimeSequential)
                {
                    file.WriteLine(item);
                }
            }
        }

        // N Vezir problemi için çözümü sağlayan özyinelemeli yardımcı fonksiyon
        bool SolveNQUtil(int[,] board, int queenCount, int row)
        {
            // Temel durum: Eğer bütün vezirler yerleştirildiyse true döndür
            if (queenCount == numQueens)
            {
                lock (lockObject)
                {
                    totalSolutions++;
                }
                return true;
            }

            bool res = false;
            // Bu satırı düşün ve sırayla bütün sütunlarda bir vezir yerleştirme
            for (int col = 0; col < N; col++)
            {
                // Vezirin board[row,col]'da yerleştirilip yerleştirilemeyeceğini kontrol et
                if (IsSafe(board, row, col))
                {
                    // Veziri board[row,col]'da yerleştir
                    board[row, col] = 1;

                    // Geri kalan vezirleri yerleştirmek için tekrar et
                    if (row + 1 < N)
                    {
                        res = SolveNQUtil(board, queenCount + 1, row + 1) || res;
                    }
                    else
                    {
                        // Eğer son sıraya ulaşıldıysa, çözümü ekrana yaz ve geri dön
                        if (queenCount == numQueens - 1)
                        {
                            lock (lockObject)
                            {
                                totalSolutions++;
                            }
                            res = true;
                        }
                    }

                    // Eğer vezirin board[row,col]'da yerleştirilmesi çözüme götürmüyorsa, veziri kaldır
                    board[row, col] = 0; // GERİ AL
                }
            }

            return res;
        }

        // Vezirin board[row,col]'da yerleştirilip yerleştirilemeyeceğini kontrol eden yardımcı fonksiyon
        bool IsSafe(int[,] board, int row, int col)
        {
            // Bu sütunda yukarıya doğru kontrol et
            for (int i = 0; i < row; i++)
            {
                if (board[i, col] == 1)
                    return false;
            }

            // Sol üst çapraz kontrolü
            for (int i = row, j = col; i >= 0 && j >= 0; i--, j--)
            {
                if (i < 0 || j < 0 || i >= N || j >= N) // Tahta sınırlarını kontrol et
                    continue;

                if (board[i, j] == 1)
                    return false;
            }

            // Sağ üst çapraz kontrolü
            for (int i = row, j = col; i >= 0 && j < N; i--, j++)
            {
                if (i < 0 || j < 0 || i >= N || j >= N) // Tahta sınırlarını kontrol et
                    continue;

                if (board[i, j] == 1)
                    return false;
            }

            return true;
        }

        // N Vezir problemi çözümünü gerçekleştiren fonksiyon
        void SolveNQ()
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            int totalTasks = N; // Her bir satır için bir Task oluştur

            // Paralel hesaplama için Task'leri kullanarak her bir satır için vezir yerleştirme işlemi yapılıyor
            Parallel.For(0, N, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (row, loopState) =>
            {
                int[,] board = new int[N, N]; // Her seferinde yeni bir tahta oluştur
                SolveNQUtil(board, 0, row);

                // Her bir satır tamamlandığında yüzdeyi hesapla ve çıktı olarak göster
                double progress = (double)Interlocked.Increment(ref totalTasks) / N * 100 - 100;
                Console.WriteLine($"İlerleme: {progress:F2}%");
            });
            stopwatch.Stop();

            if (totalSolutions == 0)
            {
                Console.WriteLine("Çözüm bulunamadı");
                return;
            }

            SaveSolutionCountAndTime(numQueens, totalSolutions, stopwatch.ElapsedMilliseconds);

            Console.WriteLine($"Toplam çözümler: {totalSolutions}");
            Console.WriteLine($"Geçen süre: {stopwatch.ElapsedMilliseconds} milisaniye");
        }

        // Aynı problemi sıralı bir şekilde çözme metodu
        void SolveNQSequential()
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            int totalTasks = N; // Her bir satır için bir Task oluştur
            double progress = 0; // İlerleme

            for (int row = 0; row < N; row++)
            {
                int[,] board = new int[N, N]; // Her seferinde yeni bir tahta oluştur
                SolveNQUtil(board, 0, row);

                // Her bir satır tamamlandığında yüzdeyi hesapla ve çıktı olarak göster
                progress = (double)Interlocked.Increment(ref totalTasks) / N * 100 - 100;
                Console.WriteLine($"İlerleme (Sıralı): {progress:F2}%");
            }
            stopwatch.Stop();

            totalSolutions *= 2;

            if (totalSolutions == 0)
            {
                Console.WriteLine("Çözüm bulunamadı");
                return;
            }

            SaveSolutionCountAndTimeSequential(numQueens, totalSolutions, stopwatch.ElapsedMilliseconds);

            Console.WriteLine($"Toplam çözümler (Sıralı): {totalSolutions}");
            Console.WriteLine($"Geçen süre (Sıralı): {stopwatch.ElapsedMilliseconds} milisaniye");
        }


        // Ana kod
        public static void Main(String[] args)
        {
            Console.WriteLine("Tahta boyutunu girin: ");
            int boardSize = Convert.ToInt32(Console.ReadLine());

            // solve 2 to boardSize (paralel solve)
            for (int i = 2; i <= boardSize; i++)
            {
                Console.WriteLine($"Paralel çözümü {i} vezir için çalıştırılıyor:");
                Program queen = new Program(boardSize, i);
                queen.SolveNQ();
            }

            // solve 2 to boardSize (sequential solve)
            for (int i = 2; i <= boardSize; i++)
            {
                Console.WriteLine($"Sequential çözümü {i} vezir için çalıştırılıyor:");
                Program queen = new Program(boardSize, i);
                queen.SolveNQSequential();
            }

            // save data to csv file
            SaveDataToCsvFile();
        }
    }
}
