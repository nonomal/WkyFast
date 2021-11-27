﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WkyFast.Service.Model;
using Newtonsoft.Json;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Xml.Linq;
using System.Diagnostics;
using Flurl.Http;
using System.Threading;

namespace WkyFast.Service
{
    public class SubscriptionManager
    {
        
        private static SubscriptionManager instance = new SubscriptionManager();

        public static SubscriptionManager Instance
        {
            get
            {
                return instance;
            }
        }

        private string _user;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();


        /// <summary>
        /// 来自账户，根据不同账户订阅不同？
        /// </summary>
        public string User {
            get
            {
                return _user;
            }
            set
            {
                _user = value;
                Load();
                Start();
            }
        }

        public List<SubscriptionModel> SubscriptionModel { get; set; } = new List<SubscriptionModel>();


        public void Start()
        {
            if (_tokenSource != null) //TODO 停止任务
            {
                _tokenSource.Cancel();
            }

            _tokenSource = new CancellationTokenSource();
            Task.Run(() => TimerFunc(_tokenSource.Token), _tokenSource.Token);
            
        }

        private void TimerFunc(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                CheckSubscription();
                Thread.Sleep(1000 * 60 * 5);
            }

        }


        /// <summary>
        /// 检查一次订阅
        /// </summary>
        private void CheckSubscription()
        {
            Debug.WriteLine("CheckSubscription");
            foreach (var subscription in SubscriptionModel)
            {
                string url = subscription.Url;
                XmlReader reader = XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();

                subscription.Name = feed.Title.Text;
                foreach (SyndicationItem item in feed.Items)
                {
                    string subject = item.Title.Text;
                    string summary = item.Summary.Text;
                    

                    foreach (var link in item.Links)
                    {
                        if (link.RelationshipType == "enclosure" || 
                            (!string.IsNullOrWhiteSpace(link.MediaType) && link.MediaType.Contains("bittorrent")))
                        {
                            //"application/x-bittorrent"
                            string downloadUrl = link.Uri.ToString();
                            //如果没有下载过
                            if (!subscription.AlreadyAddedDownloadURL.Any(a => a.Contains(downloadUrl)))
                            {
                                try
                                {
                                    //TODO 开始下载
                                    Debug.WriteLine($"添加下载{subject} {link}");
                                    if (WkyApiManager.Instance.DownloadBtFileUrl(downloadUrl, subscription.Path).Result)
                                    {
                                        subscription.AlreadyAddedDownloadURL.Add(downloadUrl);
                                        Debug.WriteLine($"添加成功");
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"添加失败");
                                        //下载失败
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                }

                            }
                        }
                    }

                    //foreach (SyndicationElementExtension extension in item.ElementExtensions)
                    //{
                    //    XElement ele = extension.GetObject<XElement>();

                    //    Debug.WriteLine("节点名称:" + ele.Name);
                        
                    //    foreach (XElement node in ele.Nodes())
                    //    {
                    //        Debug.WriteLine(node.Name.LocalName + " " + node.Value);
                    //        if (node.Name.LocalName == "link")
                    //        {
                                
                    //        }
                    //    }

                    //}

                }
                Save();
            }
        }



        public void Load()
        {
            string fileName = @$"Subscription_{_user}.json";
            Debug.WriteLine($"准备载入{fileName}");
            if (File.Exists(fileName))
            {
                SubscriptionModel.Clear();
                List<SubscriptionModel> subscriptionModel = JsonConvert.DeserializeObject<List<SubscriptionModel>>(File.ReadAllText(fileName));
                SubscriptionModel.AddRange(subscriptionModel);
            }
        }


        public void Save()
        {
            string fileName = @$"Subscription_{_user}.json";
            var content = JsonConvert.SerializeObject(SubscriptionModel);
            File.WriteAllText(fileName, content);
        }

        //存储订阅，读取加载订阅

        public bool Add(string url, string keyword = "", bool keywordIsRegex = false)
        {
            if (SubscriptionModel.Find( a => { return a.Url == url; }) != null)
            {
                //找到了存在相同
                return false;
            }

            SubscriptionModel model = new SubscriptionModel();
            model.Url = url;
            SubscriptionModel.Add(model);
            Save();
            CheckSubscription();
            return true;
        }

        public void Remove(string url)
        {
            SubscriptionModel.RemoveAll(a =>  a.Url == url);
        }

    }
}