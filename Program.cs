using Humanizer.Bytes;
using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace DiskClean
{
    class Program
    {
        private static string _pathLog = @"F:\LogsTeste\" + DateTime.Now.ToString("dd-MM-yyyy-T-HH-mm-ss");
        private static string _limitUsagePercent = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build().GetSection("CleanBySpace")["DiskLimitUsageInPercent"].ToString();
        private static string _pathToClean = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build().GetSection("CleanBySpace")["PathsToClean"];
        private static string _drive = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build().GetSection("CleanBySpace")["Drive"];

        static void Main(string[] args)
        {
            string[] _logMessage = new string[7];

            DriveInfo directoryInfo = new DriveInfo(_drive);
            Console.WriteLine("Iniciando Processo!");
            Console.WriteLine("Espaço Livre: " + ((int)directoryInfo.TotalFreeSpace));
            Console.WriteLine("Espaço total: " + directoryInfo.VolumeLabel);
            var maxFileSize = ByteSize.FromBytes(directoryInfo.TotalSize);
            var freeEspace = ByteSize.FromBytes(directoryInfo.TotalFreeSpace);
            var UsedEspace = maxFileSize - freeEspace;

            var percentUsedEspace = (UsedEspace.Gigabytes / maxFileSize.Gigabytes * 100);
            double percentUsed = Convert.ToDouble(ByteSize.FromGigabytes(percentUsedEspace).ToString("0.00000000").Replace(" GB", ""));

            DirectoryInfo info = new DirectoryInfo(_pathToClean);

            if (percentUsed >= Convert.ToDouble(_limitUsagePercent))
            {
                _logMessage[0] = "Espaço Total: " + maxFileSize;
                _logMessage[1] = "Espaço usado: " + freeEspace;
                _logMessage[2] = "% usado: " + string.Concat(percentUsed, "%");
                _logMessage[3] = "Espaço em disco acima do limite desejado!";


                var files = info.GetFiles().OrderBy(p => p.CreationTime);
                int deletedAudios = 0;
                foreach (var audio in files)
                {
                    audio.Delete();
                    deletedAudios++;

                    DriveInfo drive = new DriveInfo(_drive);
                    ByteSize maxdriveSize = ByteSize.FromBytes(drive.TotalSize);
                    ByteSize freedriveEspace = ByteSize.FromBytes(drive.TotalFreeSpace);

                    var EspaceUsed = maxdriveSize - freedriveEspace;
                    double UsedEspaceActual = (EspaceUsed.Gigabytes / maxdriveSize.Gigabytes * 100);
                    double percentdriveUsed = Convert.ToDouble(ByteSize.FromGigabytes(UsedEspaceActual).ToString("0.000000").Replace(" GB", ""));
                    if ((percentdriveUsed - 10) < Convert.ToDouble(_limitUsagePercent))
                    {
                        _logMessage[4] = deletedAudios + " arquivos excluídos.";
                        _logMessage[5] = "Novo espaço usado: " + UsedEspaceActual.ToString("0.00");
                        _logMessage[6] = "Novo % usado: " + string.Concat((percentdriveUsed - 10).ToString("0.00"), "%");
                        break;
                    }
                }

                LogWrite(_logMessage);
            }
            else
            {
                _logMessage[0] = "Espaço Total: " + maxFileSize;
                _logMessage[1] = "Espaço usado: " + UsedEspace;
                _logMessage[2] = "% usado: " + string.Concat(percentUsed.ToString("0.00"), "%");
                _logMessage[3] = "Espaço em disco dentro do limite desejado!";
                _logMessage[4] = "Nenhuma ação necessária!";
                LogWrite(_logMessage);
            }
        }

        public static void LogWrite(string[] logMessage)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_pathLog + DateTime.Now.ToString("dd-MM-yyyy-T-HH-mm-ss") + "_log.txt"))
                {
                    writer.WriteLine("Iniciando analise de espaço em disco");
                    for (int i = 0; i < logMessage.Length; i++)
                    {
                        writer.WriteLine(logMessage[i]);
                    }
                    writer.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocorreu um erro ao gerar o log: " + ex.Message);
            }

        }
    }
}
