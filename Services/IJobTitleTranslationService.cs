namespace Automind.CadastroColaboradores.Services;

public interface IJobTitleTranslationService
{
    string? Translate(string? portugueseTitle);
}
