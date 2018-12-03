using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Microsoft.Bot.Sample.LuisBot
{
    
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    { 
        // CONSTANTS        
        // Entity
        public const string Entity_Location = "位置";
        public const string Entity_Running = "办学";
        public const string Entity_Project = "工程";
        public const string Entity_Name = "院校名称";
        public const string Entity_Belong = "隶属";
    
        // Intents
        public const string Intent_AskLocation = "查询位置";
        public const string Intent_AskRunning = "查询办学";
        public const string Intent_AskProject = "查询工程";
        public const string Intent_AskType = "查询类型";
        public const string Intent_AskBelong = "查询隶属";
        public const string Intent_None = "None";
        
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }
        
        [LuisIntent(Intent_AskLocation)]
        public async Task AskLocationIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }
        
        [LuisIntent(Intent_AskRunning)]
        public async Task AskRunningIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }
        
        [LuisIntent(Intent_AskProject)]
        public async Task AskProjectIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }
        
        [LuisIntent(Intent_AskType)]
        public async Task AskTypeIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }
        
        [LuisIntent(Intent_AskBelong)]
        public async Task AskBelongIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        
        public string BotEntityRecognition(LuisResult result)
        {
            StringBuilder entityResults = new StringBuilder();

            if(result.Entities.Count>0)
            {
                foreach (EntityRecommendation item in result.Entities)
                {
                    // Query: Turn on the [light]
                    // item.Type = "HomeAutomation.Device"
                    // item.Entity = "light"
                    entityResults.Append(item.Type + "=" + item.Entity + ",");
                }
                // remove last comma
                entityResults.Remove(entityResults.Length - 1, 1);
            }
            return entityResults.ToString().Replace(" ", "");
        }
        
        public string GetCollegeName(LuisResult result)
        {
            foreach(EntityRecommendation item in result.Entities)
            {
                if(item.Type == "院校名称")
                {
                    return item.Entity;
                }
            }
            return null;
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result) 
        {
            // get recognized entities
            string entities = this.BotEntityRecognition(result);
            
            // round number
            string roundedScore =  result.Intents[0].Score != null ? (Math.Round(result.Intents[0].Score.Value, 2).ToString()) : "0";
            
            string name = this.GetCollegeName(result);
            string intent0 = result.Intents[0].Intent;
            string answer;
            
            if(intent0 != "None" && name != null)
            {
                try 
                { 
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    builder.DataSource = "misakadb.database.windows.net"; 
                    builder.UserID = "misaka";            
                    builder.Password = "FV215b183";     
                    builder.InitialCatalog = "MisakaDB";
    
                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        connection.Open();       
                        string sql = "SELECT * FROM dbo.Colleges WHERE 名称 = \'" + name + "\'";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if(intent0 == "查询位置")
                                    {
                                        answer = name + "位于" + reader.GetString(1);
                                    }
                                    else if(intent0 == "查询办学")
                                    {
                                        answer = name + "是";
                                        if(reader.GetString(4) == "是")
                                        {
                                            answer += "公办院校";
                                        }
                                        else
                                        {
                                            answer += "民办院校";    
                                        }
                                    }
                                    else if(intent0 == "查询类型")
                                    {
                                        answer = name + "是" + reader.GetString(5);
                                    }
                                    else if(intent0 == "查询隶属")
                                    {
                                        answer = name + "隶属于" + reader.GetString(7);
                                    }
                                    else    //查询工程
                                    {
                                        if(result.Entities[0].Entity == "985" || result.Entities[0].Entity == "985工程" || result.Entities[0].Entity == "九八五")
                                        {
                                            if(reader.GetString(3) == "是")
                                            {
                                                answer = name + "是985工程院校";
                                            }
                                            else
                                            {
                                                answer = name + "不是985工程院校";
                                            }
                                        }
                                        else
                                        {
                                            if(reader.GetString(2) == "是")
                                            {
                                                answer = name + "是211工程院校";
                                            }
                                            else
                                            {
                                                answer = name + "不是211工程院校";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    answer = "对不起，没有找到该院校";    
                                }
                            }
                        }                    
                    }
                }
                catch (SqlException e)
                {
                    answer = e.ToString();
                }
            }
            else
            {
                answer = "对不起，我无法理解您的要求";
            }
            
            //await context.PostAsync($"**Query**: {result.Query}, **Intent**: {result.Intents[0].Intent}, **Score**: {roundedScore}. **Entities**: {entities}");
            await context.PostAsync(answer);
            context.Wait(MessageReceived);
        }
    }
}