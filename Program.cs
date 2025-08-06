// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System.Net.Http.Headers;

class Program
{
    static async Task Main()
    {
        var inputPath = "input.csv";
        var outputJsonDir = "../RWcnpjs";
        var outputCsvPath = "../csv_saida/output.csv";

        Directory.CreateDirectory(outputJsonDir);
        Directory.CreateDirectory(Path.GetDirectoryName(outputCsvPath)!);

        var rawCnpjs = File.ReadAllLines(inputPath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var cnpjs = rawCnpjs.Select(NormalizeCNPJ).ToList();

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ConsultaCNPJApp", "1.0"));

        var csvLines = new List<string> {
            "cnpj,tipo,porte,nome,fantasia,atividade_principal,atividades_secundarias,natureza_juridica,bairro,municipio,uf,situacao,simples_optante,simei_optante"
        };

        foreach (var cnpj in cnpjs)
        {
            try
            {
                var url = $"https://receitaws.com.br/v1/cnpj/{cnpj}";
                var response = await httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                var jsonPath = Path.Combine(outputJsonDir, $"ReceitaWS_{cnpj}.json");
                await File.WriteAllTextAsync(jsonPath, json);

                // Aqui você pode reativar o trecho de parsing do JSON se quiser gerar o CSV
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar CNPJ {cnpj}: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(20)); // 3 consultas por minuto
        }

        //await File.WriteAllLinesAsync(outputCsvPath, csvLines);
        Console.WriteLine("Processamento concluído.");
    }

    static string NormalizeCNPJ(string rawCnpj)
    {
        var digitsOnly = new string(rawCnpj.Where(char.IsDigit).ToArray());
        return digitsOnly.PadLeft(14, '0');
    }

    static string Quote(string? value) => $"\"{value?.Replace("\"", "\"\"")}\"";
}
