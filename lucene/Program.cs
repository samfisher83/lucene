using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lucene.Net.Store;
//using Lucene.Net.Analysis.Standard.StandardAnalyzer;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System.IO;
using System.Diagnostics;
using System.Web;
using System.Web.Script.Serialization;


namespace WindowsFormsApplication1
{
    public class text_score {
        public string question;
        public int question_id;
        public string[] text; 
        public float[] score;
        public int[] doc;

        public text_score(int size)
        {
            //question = new string[size];
            text = new string[size];
            score = new float[size];
            doc = new int[size];

            
        }
        public text_score()
        {
        }
        public void computeTotal(){
            totalScore = 0;
            float baseScore = score[0];

            for (int i = 0; i < score.Length; i++)
            {
                score[i] = score[i] / baseScore;
            }
            
            
            
            
            
            for (int i = 0; i < score.Length; i++)
            {
                totalScore += score[i];
                
            }   
        
        }

        public float totalScore = 0;
    }
    public class array_text_score
    {
        public text_score[] array_text_scores = new text_score[5];
    }
    public class text_finalScore
    {
        public int docid;
        public string text;
        public float total;
        public string relateddocs;
        public string relatedscores;
    }
    
    
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {




            luceneExample();
            //
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }


        private static void luceneExample()
        {
            Lucene.Net.Store.Directory index = new RAMDirectory();
            StandardAnalyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);



            OpenFileDialog open = new OpenFileDialog();
            open.ShowDialog();
            List<string> questions = new List<string>();
            List<text_score> text_scores = new List<text_score>();
            //JavaScriptSerializer jsonIn = new JavaScriptSerializer();
            //using (StreamReader inHandle = new StreamReader(open.FileName))
            //{
            //    questions = jsonIn.Deserialize<List<string>>(inHandle.ReadToEnd());
         
            //}


            
            //open.ShowDialog();

            //using (StreamReader inHandle = new StreamReader(open.FileName))
            //{
            //    questions.AddRange(jsonIn.Deserialize<List<string>>(inHandle.ReadToEnd()));

            //}

            
            //string line;
            //using(StreamReader inHandle = new StreamReader(open.FileName)){
            //    while ((line = inHandle.ReadLine()) != null)
            //    {
            //        questions.Add(line);
            //    }
            
            //}

            using (StreamReader inHandle = new StreamReader(open.FileName))
            {
                System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
                questions = xml.Deserialize(inHandle) as List<string>;

            }


            
            IndexWriter w = new IndexWriter(index, analyzer);


            foreach (string item in questions)
            {
                addDoc(w, item);
            }


            //addDoc(w, "Lucene in Action");
            //addDoc(w, "Lucene for Dummies");
            //addDoc(w, "Managing Gigabytes");
            //addDoc(w, "The Art of Computer Science");
            w.Close();
            Stopwatch time = new Stopwatch();
            time.Start();

            int hitsize = 30;
            
            using (StreamWriter outhandle = new StreamWriter("out.txt"))
            {
                for (int i = 0; i < questions.Count; i++)
                {
                    System.Console.WriteLine("{0}/{1}", i, questions.Count);
                    String querystr = questions[i];
                    Query q = null;
                    try
                    {
                        q = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "title", analyzer).Parse(QueryParser.Escape(querystr));
                    }
                    catch (Exception err)
                    {
                        System.Console.WriteLine(err.Message);
                        goto skip;

                    }
                    //q.Parse();


                    int hitsPerPage = hitsize;
                    IndexReader reader = IndexReader.Open(index, true);
                    IndexSearcher searcher = new IndexSearcher(reader);
                    TopScoreDocCollector collector = TopScoreDocCollector.Create(hitsPerPage, true);
                    searcher.Search(q, collector);
                    ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

                    //outhandle.Write("Found {0} Hits|", hits.Length);
                    text_score temp = new text_score(hitsize);
                    temp.question =  questions[i];
                    temp.question_id = i;
                    int j = 0;
                    foreach (var item in hits)
                    {
                        int docId = item.Doc;
                        Document d = searcher.Doc(docId);
                        //temp.array_text_scores[j] = new text_score();
 
                        temp.text[j] = d.Get("title");
                        temp.score[j] = item.Score;
                        temp.doc[j] = docId;
                        


                        
                        
                        
                        
                        //outhandle.Write( d.Get("title").Replace("\n"," ") + "|" + item.Score + "|");
                        j++;
                    }
                    temp.computeTotal();
                    text_scores.Add(temp);
                    outhandle.WriteLine();
                    

                skip:
                    int k = 0;
                }
                time.Stop();
                System.Console.WriteLine(time.ElapsedMilliseconds);
            }
            using (StreamWriter outh = new StreamWriter(open.FileName.Split('.').First() + ".full.xml"))
            {
                System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<text_score>));
                xml.Serialize(outh, text_scores);

            }
            outputResults(open, text_scores,"results");

            removeSimilar(text_scores);

            outputResults(open, text_scores, "reduced");
            



        }
        /// <summary>
        /// This function puts into a format excel will show correctly
        /// </summary>
        /// <param name="open"></param>
        /// <param name="text_scores"></param>
        private static void outputResults(OpenFileDialog open, List<text_score> text_scores, string results)
        {
            List<text_finalScore> outFinal = new List<text_finalScore>();
            foreach (text_score item in text_scores)
            {
                outFinal.Add(new text_finalScore()
                {
                    text = item.question,
                    total = item.totalScore,
                    docid = item.question_id,
                    relateddocs = string.Join(",", item.doc),
                    relatedscores = string.Join(",", item.score)
                });

            }
            outFinal = outFinal.OrderByDescending(x => x.total).ToList();


            using (StreamWriter outh = new StreamWriter(open.FileName.Split('.').First() + "." + results + ".xml"))
            {
                System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<text_finalScore>));
                xml.Serialize(outh, outFinal);

            }
        }

        private static void  removeSimilar(List<text_score> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                float basescore = items[i].score[0];
                for (int j = 1; j < items[i].doc.Length; j++)
                {
                    if (Math.Abs(items[i].score[j] / basescore - 1.0) < .30)
                    {
                        text_score temp;
                        try
                        {
                            temp = items.First(x => x.question_id == items[i].doc[j]);
                        }
                        catch (Exception err)
                        {
                            temp = null;
                        }
                        if(temp!=null){
                            items.Remove(temp);
                        }
                    }
                }
            }
        }

        private static void addDoc(IndexWriter w, String value)
        {
            Document doc = new Document();

            doc.Add(new Field("title", value, Field.Store.YES, Field.Index.ANALYZED));
            w.AddDocument(doc);
        }
    }
}
