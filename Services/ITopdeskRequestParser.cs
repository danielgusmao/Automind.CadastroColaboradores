using Automind.CadastroColaboradores.Models;

namespace Automind.CadastroColaboradores.Services;

public interface ITopdeskRequestParser
{
    TopdeskFormData Parse(string request);
}
