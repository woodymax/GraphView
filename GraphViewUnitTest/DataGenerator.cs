﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace GraphViewUnitTest
{
    public interface IDistribution
    {
        int GetOutDegreeNum(double rdNum);
        long GetOutDegreeId(int nodeSize, int edgeNum, int curIndex, Random rand);
    }

    public class Unifrom : IDistribution
    {
        private readonly int _aveDegree;

        public Unifrom(int aveDegree)
        {
            _aveDegree = aveDegree;
        }

        public int GetOutDegreeNum(double rdNum)
        {
            return (int)(rdNum*2*_aveDegree);
        }

        public long GetOutDegreeId(int nodeSize, int edgeNum, int curIndex, Random rand)
        {
            double ratio = nodeSize*1.0/edgeNum;
            var begin = (int) Math.Ceiling(curIndex*ratio);
            var end = (int) Math.Floor((curIndex + 1)*ratio) + 1;
            return (long)(rand.Next(begin, end));
        }
    }

    public class Pareto : IDistribution
    {
        private readonly double _k;
        private const double Xmin = 20;
        private readonly int _maxDegree;
        private static int _count = 0;

        public Pareto(double aveDegree, int maxDegree)
        {
            _k = (aveDegree + Xmin)/(aveDegree);
            _maxDegree = maxDegree;
        }

        public double ParetoDistribution(double x, double k, double xmin)
        {
            return xmin/Math.Pow(1 - x, 1/k) - xmin;
        }

        public int GetOutDegreeNum(double rdNum)
        {
            var num = (int) ParetoDistribution(rdNum, _k, Xmin);
            if (num > _maxDegree)
                num = _maxDegree;
            return num;
        }

        public long GetOutDegreeId(int nodeSize, int edgeNum, int curIndex, Random rand)
        {
            //double midSize = nodeSize/2.0;
            //var kid = (midSize+Xmin)/(midSize);
            if (curIndex == 0) _count = 0;
            var xmin = nodeSize*0.04;
            var scale = 1 - Math.Pow(Xmin/nodeSize,_k);
            var begin = (int)Math.Floor(ParetoDistribution(curIndex * scale / (edgeNum), _k, Xmin)) + _count;
            var end = curIndex == edgeNum - 1
                ? nodeSize - 1
                : (int) Math.Floor(ParetoDistribution((curIndex + 1)*scale/(edgeNum), _k, Xmin)) + _count;
            if (begin == end)
            {
                end++;
                _count++;
            }
            return
                (long)rand.Next(begin < nodeSize && begin >= 0 ? begin : nodeSize - 1, end < nodeSize && end >= 0 ? end: nodeSize - 1);
            //double midSize = nodeSize/2.0;
            //var K = (midSize+Xmin)/(midSize);
            //var result = (long)(Xmin/Math.Pow(1 - rand, 1/K) - Xmin);
            //if (result - Xmin > nodeSize-1)
            //    return nodeSize-1;
            //return result - (long)Xmin;

        }
    }

    class DataGenerator
    {
        private const int EmployeeNodeSize = 100;
        private const int ClientNodeSize = 100;
        private const int AverageDegree = 40;
        private const int MaxDegree = EmployeeNodeSize;
        private static int Ran = new Random().Next(0, EmployeeNodeSize);
        private static int _rep = 0;

        private static readonly IDistribution Distribution =
            new Unifrom(AverageDegree);
        //new Pareto(AverageDegree,MaxDegree);
        enum Edge
        {
            EmployeeColleagues = 0,
            EmployeeClients = 1,
            EmployeeManager = 2,
            ClientColleagues = 3,
        }

        private static string RandomString(int strLength = 10)
        {
            var rndStr = "";
            var tick = (int)DateTime.Now.Ticks + _rep++;
            var rd = new Random(tick);

            for (var i = 0; i < strLength; ++i)
            {
                var rndChr = (char)rd.Next(65, 90);
                rndStr = rndStr + rndChr;
            }
            return rndStr;
        }

        //private static byte[] GenerateBinary(int num, Random rdInt, Edge type )
        //{
        //    var ret = new MemoryStream();
        //    var br = new BinaryWriter(ret);
        //    for (var j = 0; j < num; ++j)
        //    {
        //        var ratio = (double)EmployeeNodeSize / num;
        //        var next = rdInt.Next((int)Math.Ceiling(j * ratio), (int)Math.Floor((j + 1) * ratio) + 1);
        //        var next64 = (Int64)next;

        //        if (type == Edge.Clients)
        //        {
        //            br.Write(next64 + (Convert.ToInt64(1)<<48));
        //            br.Write(next);
        //            br.Write(rdInt.NextDouble());
        //            br.Write(RandomString());
        //        }
        //        else
        //        {
        //            br.Write(next64);
        //        }
        //    }
        //    return ret.ToArray();
        //}
        private static byte[] GenerateBinary(int dstNodeSize, Random rd, Edge type, out int actualEdgeNum)
        {

            var ret = new MemoryStream();
            var br = new BinaryWriter(ret);
            var edgeNum = (int)(Distribution.GetOutDegreeNum(rd.NextDouble()));
            var tick = (int)DateTime.Now.Ticks + _rep++;
            var rd2 = new Random(tick);
            long pre64 = -1;
            actualEdgeNum = 0;

            for (var j = 0; j < edgeNum; ++j)
            {
                //var ratio = (double)EmployeeNodeSize / edgeNum;
                //var next = rdInt.Next((int)Math.Ceiling(j * ratio), (int)Math.Floor((j + 1) * ratio) + 1);
                long next64 = Distribution.GetOutDegreeId(dstNodeSize, edgeNum, j, rd2) + Ran;
                if (pre64 == next64)
                    continue;
                pre64 = next64;
                if (next64 > dstNodeSize - 1)
                {
                    next64 = next64 - dstNodeSize;
                }
                Int32 next = rd.Next(50);

                if (type == Edge.EmployeeClients)
                {
                    byte[] w = new byte[1];
                    w[0] = 7;
                    br.Write(w);
                    br.Write(next64 + (Convert.ToInt64(1) << 48));
                    br.Write(next);
                    br.Write(rd.NextDouble());
                    br.Write(RandomString());
                }
                else if (type == Edge.ClientColleagues)
                {
                    br.Write(next64 + (Convert.ToInt64(1) << 48));
                }
                else
                {
                    br.Write(next64);
                }
                actualEdgeNum++;
            }
            return ret.ToArray();
        }

        public static void InsertDataEmployNode(SqlConnection conn) // binary version
        {
            using (var cmd = conn.CreateCommand())
            {
                // Start Data Generation
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 128);
                cmd.Parameters.Add("@WorkId", SqlDbType.NVarChar, 128);
                cmd.Parameters.Add("@Clients", SqlDbType.VarBinary, -1);
                cmd.Parameters.Add("@Colleagues", SqlDbType.VarBinary, -1);
                cmd.Parameters.Add("@Manager", SqlDbType.VarBinary, -1);
                cmd.Parameters.Add("@ClientsOutDegree", SqlDbType.Int);
                cmd.Parameters.Add("@ColleaguesOutDegree", SqlDbType.Int);
                cmd.Parameters.Add("@ManagerOutDegree", SqlDbType.Int);

                var tick = (int)DateTime.Now.Ticks + _rep++;
                var rd = new Random(tick);

                for (var i = 0; i < EmployeeNodeSize; ++i)
                {
                    var name = RandomString();
                    var workId = RandomString();


                    //var numberOfClients = rdInt.Next(1, 2 * AverageDegree);
                    //var numberOfColleagues = rdInt.Next(1, 2 * AverageDegree);
                    //var numberOfManager = rdInt.Next(1, 2 * AverageDegree);

                    //cmd.CommandText = "insert EmployeeNode values(" + i.ToString() + ", '" + Name + "', @Clients, @Colleagues, @Friends, " + "0)";
                    cmd.CommandText =
                        @"INSERT EmployeeNode (WorkId, name, Colleagues, Clients, Manager,ColleaguesOutDegree, ClientsOutDegree, ManagerOutDegree) Values (@WorkId, @name, @Colleagues, @Clients, @Manager,@ColleaguesOutDegree, @ClientsOutDegree, @ManagerOutDegree)";

                    cmd.Parameters["@name"].Value = name;
                    cmd.Parameters["@WorkId"].Value = workId;
                    //cmd.Parameters["@Clients"].Value = GenerateBinary(numberOfClients, rdInt, Edge.Clients);
                    //cmd.Parameters["@Colleagues"].Value = GenerateBinary(numberOfColleagues, rdInt, Edge.Colleagues);
                    //cmd.Parameters["@Manager"].Value = GenerateBinary(numberOfManager, rdInt, Edge.Manager);

                    int edgeNum;
                    cmd.Parameters["@Colleagues"].Value = GenerateBinary(EmployeeNodeSize, rd, Edge.EmployeeColleagues, out edgeNum);
                    cmd.Parameters["@ColleaguesOutDegree"].Value = edgeNum;

                    cmd.Parameters["@Clients"].Value = GenerateBinary(ClientNodeSize, rd, Edge.EmployeeClients, out edgeNum);
                    cmd.Parameters["@ClientsOutDegree"].Value = edgeNum;

                    cmd.Parameters["@Manager"].Value = GenerateBinary(EmployeeNodeSize, rd, Edge.EmployeeManager, out edgeNum);
                    cmd.Parameters["@ManagerOutDegree"].Value = edgeNum;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        //Console.WriteLine("RowsAffected: {0}", rowsAffected);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            //End Data Generation
        }

        public static void InsertDataClientNode(SqlConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                //Start Data Generation
                var tick = (int)DateTime.Now.Ticks + _rep++;
                var rd = new Random(tick);
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 128);
                cmd.Parameters.Add("@Colleagues", SqlDbType.VarBinary, -1);
                cmd.Parameters.Add("@ClientId", SqlDbType.NVarChar, 128);
                cmd.Parameters.Add("@ColleaguesOutDegree", SqlDbType.Int);

                for (var i = 0; i < ClientNodeSize; ++i)
                {
                    var name = RandomString();
                    cmd.CommandText = "INSERT ClientNode (ClientId, name, Colleagues,ColleaguesOutDegree) VALUES (@ClientId, @name, @Colleagues,@ColleaguesOutDegree)";
                    cmd.Parameters["@name"].Value = name;
                    //cmd.Parameters["@Colleagues"].Value = GenerateBinary(numberOfColleagues, rdInt, Edge.Colleagues);
                    cmd.Parameters["@ClientId"].Value = RandomString();
                    int edgeNum;

                    cmd.Parameters["@Colleagues"].Value = GenerateBinary(ClientNodeSize, rd, Edge.ClientColleagues,out edgeNum);
                    cmd.Parameters["@ColleaguesOutDegree"].Value = edgeNum;

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
