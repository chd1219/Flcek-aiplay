using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;
using RedisStudy;
using System.Net;
using System.IO;

namespace Fleck.aiplay
{
    class RedisHelper
    {
        IRedisClient Redis;
        HashOperator operators;

        public RedisHelper()
        {
            InitRedis();
        }

        public void InitRedis()
        {
            //获取Redis操作接口
            Redis = RedisManager.GetClient();
            //Hash表操作
            operators = new HashOperator();

            Redis.Password = "jiao19890228";
        }

        public string getFromRedis(string key)
        {
            lock (Redis)
            {
                return Redis.GetValue(key);
            }

        }

        public bool ContainsKey(string key)
        {
            lock (Redis)
            {
                if (GetListCount(key) >= Int32.Parse(Setting.level))
                {
                    return true;
                }
                Redis.RemoveAllFromList(key);
                return false;
            }

        }

        public void setToRedis(string key, string value)
        {
            lock (Redis)
            {
                Redis.SetEntryIfNotExists(key, value);
            }
        }

        public int GetListCount(string listId)
        {
            lock (Redis)
            {
                return Redis.GetListCount(listId);
            }
        }

        public List<string> GetAllItemsFromList(string listId)
        {
            lock (Redis)
            {
                return Redis.GetAllItemsFromList(listId);
            }
        }
        
        public string GetItemFromList(string listId, int listIndex)
        {
            lock (Redis)
            {
                return Redis.GetItemFromList(listId, listIndex);
            }
        }
        
        public void CheckItemInList(string listId, int count)
        {
            lock (Redis)
            {
                for (int i = Redis.GetListCount(listId)-1; i < count; i++)
                {
                    Redis.AddItemToList(listId, "");
                }
            }
        }

        public void PushItemToList(string listId, string value)
        {
            lock (Redis)
            {
                Redis.PushItemToList(listId, value);
            }
        }
        
        public void SetItemInList(string listId, int listIndex, string value)
        {
            lock (Redis)
            {
                if (listIndex > 0 && listIndex <= Int32.Parse(Setting.level))
                {
                    Redis.SetItemInList(listId, listIndex, value);
                }                
            }
        }

        public string QuerybestFromCloud(string board)
        {
            if (!Setting.isSupportCloudApi)
            {
                return null;
            }
            string serverResult = "";
            try
            {
                serverResult = getFromRedis("Querybest:" + board);
                if (serverResult == null)
                {
                    string serverUrl = "http://api.chessdb.cn:81/chessdb.php?action=querybest&board=" + board;
                    string postData = "";
                    serverResult = HttpPostConnectToServer(serverUrl, postData);
                    if (serverResult != null)
                    {
                        setToRedis("Querybest:" + board, serverResult);
                    }
                    else
                    {
                        serverResult = "";
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[error] QuerybestFromCloud " + ex.Message);
            }
            return serverResult;
        }

        public string QueryallFromCloud(string message)
        {
            string board = message.Substring(9, message.Length - 9);
            string serverResult = "";
            try
            {
                serverResult = getFromRedis("Queryall:" + board);
                if (serverResult == null)
                {
                    string serverUrl = "http://api.chessdb.cn:81/chessdb.php?action=queryall&board=" + board;
                    string postData = "";
                    serverResult = HttpPostConnectToServer(serverUrl, postData);
                    if (serverResult != null)
                    {                        
                        serverResult = serverResult.Replace("move:", "");//替换为空
                        serverResult = serverResult.Replace("score:", "");//替换为空
                        serverResult = serverResult.Replace("rank:", "");//替换为空
                        serverResult = serverResult.Replace("note:", "");//替换为空
                        serverResult = "Queryall:" + serverResult;
                        setToRedis("Queryall:" + board, serverResult);
                    }
                    else
                    {
                        serverResult = "Queryall:null";
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[error] QueryallFromCloud " + ex.Message);
            }
            return serverResult;
        }

        public string HttpPostConnectToServer(string serverUrl, string postData)
        {
            var dataArray = Encoding.UTF8.GetBytes(postData);
            //创建请求  
            var request = (HttpWebRequest)HttpWebRequest.Create(serverUrl);
            request.Method = "POST";
            request.ContentLength = dataArray.Length;
            //设置上传服务的数据格式  
            request.ContentType = "application/x-www-form-urlencoded";
            //请求的身份验证信息为默认  
            request.Credentials = CredentialCache.DefaultCredentials;
            //请求超时时间  
            request.Timeout = 10000;
            //创建输入流  
            Stream dataStream;
            try
            {
                dataStream = request.GetRequestStream();
            }
            catch (Exception)
            {
                return null;//连接服务器失败  
            }
            //发送请求  
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();
            //读取返回消息  
            string res = "";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                res = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[error] HttpPostConnectToServer " + ex.Message);
            }
            return res;
        }
    }
}
