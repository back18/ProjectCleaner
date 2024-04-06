namespace ProjectCleaner
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            string arg = args.Length == 1 ? args[0] : string.Empty;

            while (true)
            {
                string? input;
                if (args.Length == 1)
                {
                    input = args[0];
                    args = [];
                }
                else
                {
                    Console.Write("请输入目录路径：");
                    input = Console.ReadLine();
                    Console.WriteLine();
                }

                if (string.IsNullOrEmpty(input))
                    break;

                input = Path.GetFullPath(input.Trim('"'));
                if (!Directory.Exists(input))
                {
                    Console.WriteLine($"路径“{input}”的目录不存在");
                    Console.WriteLine();
                    continue;
                }

                string[] projects = GetProjects(input);
                Console.WriteLine($"在目录“{input}”找到{projects.Length}个项目文件：");
                foreach (string project in projects)
                    Console.WriteLine(project);
                Console.WriteLine();

                Console.WriteLine("键入回车继续");
                if (Console.ReadKey(true).Key != ConsoleKey.Enter)
                {
                    Console.WriteLine("已取消");
                    Console.WriteLine();
                    continue;
                }
                else
                {
                    Console.WriteLine("开始清理所有项目");
                    Console.WriteLine();
                }

                List<CleanResult> cleanResults = [];
                foreach (string project in projects)
                {
                    CleanResult cleanResult = Clean(project);
                    cleanResults.Add(cleanResult);
                }

                int successes = cleanResults.Count(s => s == CleanResult.Success);
                int failures = cleanResults.Count(s => s == CleanResult.Failure);
                int skippeds = cleanResults.Count(s => s == CleanResult.Skipped);
                Console.WriteLine($"清理完成，共{cleanResults.Count}个项目，{successes}个成功，{failures}个失败，{skippeds}个跳过");
                Console.WriteLine();
            }
        }

        private static string[] GetProjects(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            List<string> result = [];

            string[] projects = Directory.GetFiles(path, "*.csproj");
            result.AddRange(projects);

            string[] directorys = Directory.GetDirectories(path);
            foreach (string directory in directorys)
                result.AddRange(GetProjects(directory));

            return result.ToArray();
        }

        private static CleanResult Clean(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            Console.WriteLine("正在清理项目：" + path);

            try
            {
                string? directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    throw new DirectoryNotFoundException();

                string binDirectory = Path.Combine(directory, "bin");
                string objDirectory = Path.Combine(directory, "obj");
                if (Environment.CurrentDirectory.StartsWith(directory) || !Directory.Exists(binDirectory) || !Directory.Exists(objDirectory))
                {
                    Console.WriteLine("----条件不满足，已跳过");
                    return CleanResult.Skipped;
                }

                Directory.Delete(binDirectory, true);
                Console.WriteLine("----成功删除目录：" + binDirectory);

                Directory.Delete(objDirectory, true);
                Console.WriteLine("----成功删除目录：" + objDirectory);

                return CleanResult.Success;
            }
            catch (Exception ex)
            {
                Type type = ex.GetType();
                Console.WriteLine($"----清理失败，错误信息：{type.FullName ?? type.Name}: {ex.Message}");
                return CleanResult.Failure;
            }
            finally
            {
                Console.WriteLine();
            }
        }
    }
}
