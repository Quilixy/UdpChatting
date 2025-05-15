using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UdpChatting
{
    class PlayfairCipher
    {
        private static readonly char[] Alphabet = "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZXWQ().,".ToCharArray();
        private const int Size = 6;
        private readonly char[,] matrix = new char[Size, Size];

        public PlayfairCipher(string keyword)
        {
            GenerateMatrix(keyword);
        }

        private void GenerateMatrix(string keyword)
        {
            var cleanKeyword = new string(keyword
                .ToUpper(new CultureInfo("tr-TR"))
                .Where(c => Alphabet.Contains(c))
                .Distinct()
                .ToArray());

            var fullKey = cleanKeyword + new string(Alphabet.Where(c => !cleanKeyword.Contains(c)).ToArray());

            for (int i = 0; i < Size * Size; i++)
            {
                matrix[i / Size, i % Size] = fullKey[i];
            }
        }

        private (int row, int col) FindPosition(char ch)
        {
            for (int row = 0; row < Size; row++)
                for (int col = 0; col < Size; col++)
                    if (matrix[row, col] == ch)
                        return (row, col);
            throw new Exception("Harf matris içinde bulunamadı: " + ch);
        }

        private List<(char, char)> PrepareDigraphs(string text)
        {
            List<(char, char)> digraphs = new();
            var cleanText = new string(text
                .ToUpper(new CultureInfo("tr-TR", false))
                .Where(c => Alphabet.Contains(c))
                .ToArray());

            int i = 0;
            while (i < cleanText.Length)
            {
                char a = cleanText[i];
                char b = (i + 1 < cleanText.Length) ? cleanText[i + 1] : 'X';

                if (a == b)
                {
                    digraphs.Add((a, 'X'));
                    i++;
                }
                else
                {
                    digraphs.Add((a, b));
                    i += 2;
                }
            }

            // Eğer tek karakter kaldıysa (örneğin en son 'A' tek başına)
            if (digraphs.Count > 0 && digraphs.Last().Item2 == 'X' && cleanText.Length % 2 == 0)
            {
                // bir şey yapmaya gerek yok, zaten X eklenmiş olur
            }

            return digraphs;
        }


        public string Encrypt(string plaintext)
        {
            var digraphs = PrepareDigraphs(plaintext);
            StringBuilder sb = new();

            foreach (var (a, b) in digraphs)
            {
                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                if (row1 == row2)
                {
                    sb.Append(matrix[row1, (col1 + 1) % Size]);
                    sb.Append(matrix[row2, (col2 + 1) % Size]);
                }
                else if (col1 == col2)
                {
                    sb.Append(matrix[(row1 + 1) % Size, col1]);
                    sb.Append(matrix[(row2 + 1) % Size, col2]);
                }
                else
                {
                    sb.Append(matrix[row1, col2]);
                    sb.Append(matrix[row2, col1]);
                }
            }

            return sb.ToString();
        }

        public string Decrypt(string ciphertext)
        {
            List<(char, char)> digraphs = new();
            for (int i = 0; i < ciphertext.Length; i += 2)
            {
                digraphs.Add((ciphertext[i], ciphertext[i + 1]));
            }

            StringBuilder sb = new();

            foreach (var (a, b) in digraphs)
            {
                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                if (row1 == row2)
                {
                    sb.Append(matrix[row1, (col1 + Size - 1) % Size]);
                    sb.Append(matrix[row2, (col2 + Size - 1) % Size]);
                }
                else if (col1 == col2)
                {
                    sb.Append(matrix[(row1 + Size - 1) % Size, col1]);
                    sb.Append(matrix[(row2 + Size - 1) % Size, col2]);
                }
                else
                {
                    sb.Append(matrix[row1, col2]);
                    sb.Append(matrix[row2, col1]);
                }
            }

            return sb.ToString();
        }

        public void PrintMatrix()
        {
            Console.WriteLine("Playfair Matrisi:");
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                    Console.Write(matrix[i, j] + " ");
                Console.WriteLine();
            }
        }
    }
}
