using UnityEngine;
using System.IO;
using Newtonsoft.Json; 

public class DataFileManager 
{
    // Nome do ficheiro
    private const string SAVE_FILE_NAME = "survivorData.json";

    // Instância global para ser fácil aceder de outros scripts
    private static DataFileManager instance  = null;

    // A variável que guarda os dados em memória
    public DataFileRoot currentData;

    private DataFileManager(){}

    public static DataFileManager GetInstance()
    {
        return instance ??= new DataFileManager();
    }


    private void SaveData()
    {
        // 1. Define o caminho correto para o Quest 3
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        // 2. Serializa o objeto para texto JSON (formatado para leitura fácil)
        string jsonString = JsonConvert.SerializeObject(currentData, Formatting.Indented);

        // 3. Escreve no disco
        try
        {
            File.WriteAllText(path, jsonString);
            Debug.Log($"[SaveManager] Dados gravados com sucesso em: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Erro ao gravar: {e.Message}");
        }
    }

    public void LoadData()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        if (File.Exists(path))
        {
            try
            {
                string jsonString = File.ReadAllText(path);
                currentData = JsonConvert.DeserializeObject<DataFileRoot>(jsonString);
                Debug.Log("[SaveManager] Dados carregados.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Erro ao ler ficheiro, a criar novo: {e.Message}");
                currentData = new DataFileRoot();
            }
        }
        else
        {
            Debug.Log("[SaveManager] Ficheiro não encontrado. A criar novo perfil.");
            currentData = new DataFileRoot();
        }
    }

    // --- EXEMPLO DE COMO ADICIONAR DADOS ---
    
    // Método para ser chamado pelo seu script CannyWise
    public void SaveSessionData(SessionData newSession, string sessionID, string miniGameName)
    {
        if (miniGameName == "frisbee")
        {
            if (currentData.frisbeeGame.SessionsData.ContainsKey(sessionID))
            {
                currentData.frisbeeGame.SessionsData[sessionID] = newSession; 
            }
            else
            {
                currentData.frisbeeGame.SessionsData.Add(sessionID, newSession); // Cria
            }
                
        }
        else if (miniGameName == "archery")
        {
            if (currentData.archeryGame.SessionsData.ContainsKey(sessionID))
            {
                currentData.archeryGame.SessionsData[sessionID] = newSession;
            }
            else
            {
                currentData.archeryGame.SessionsData.Add(sessionID, newSession);
            }
        }
        else
        {
            Debug.LogError($"Invalid Mini Game Name: {miniGameName}");
            return;
        }

        SaveData(); 
    }
}