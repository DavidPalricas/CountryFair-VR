using System.Collections.Generic;

[System.Serializable]
public class MiniGameData
{
    // Usamos Dictionary para ter chaves dinâmicas como "sesson1", "sesson2"
    public Dictionary<string, SessionData> SessionsData { get; set; } = new();
    
    // Dicionário para parâmetros adaptativos (flexível)
    public Dictionary<string, object> AdadaptiveParameters { get; set; } = new();

    // Construtor para inicializar os dicionários e evitar erros de null
    public MiniGameData()
    {
        SessionsData = new Dictionary<string, SessionData>();
        AdadaptiveParameters = new Dictionary<string, object>();
    }
}