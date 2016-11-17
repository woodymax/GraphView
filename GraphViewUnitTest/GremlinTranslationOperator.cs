﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using GraphView.GremlinTranslationOps;

namespace GremlinTranslationOperator.Tests
{
    [TestClass()]
    public class GremlinTranslationOperator
    {
        [TestMethod()]
        public void nextTest()
        {
            GraphTraversal2 g = new GraphTraversal2();
            g.V().As("a").Out().As("b").addE("123").@from("a").next(); //pass
            //g.V().As("a").Out().As("b").addE("123").@from("a").to("b").next(); //pass
            //g.V().addV().next(); //pass
            //g.V().addV().property("name", "jinjin").next(); //pass

            // TODO: add property 
            //g.V().property("name", "jinjin").next();

            //g.constant("123").next(); //pass
            //g.V().drop().next(); //pass
            //g.E().drop().next(); //pass
            //g.V().has("name", "jinjin").next(); //pass
            //g.V().group().@by("name").next(); //pass
            //g.V().order().@by("name").next(); //pass
            //g.V().order().@by("name").@by("age").next(); //pass
            //g.V().Out().next(); //pass
            //g.V().Out("create").next(); //pass
            //g.V().outE().next(); //pass
            //g.V().outE("create").next(); //pass
            //g.V().In().next(); //pass
            //g.V().In("create").next(); //pass
            //g.V().inE().next(); //pass
            //g.V().inE("create").next(); //pass
            //g.E().bothV().next(); //pass

            //g.V().bothE().next();

            //g.V().bothE("create").next(); //pass
            //g.V().count().next();  //pass
            //g.V().values("age").next(); //pass
            //g.V().values("name", "age").next(); //pass
            g.V().values("age").max().next();
            g.V().values("age").min().next();
            g.V().values("age").mean().next();
            //g.V().fold().next();
            //g.V().fold().unfold().next();

            //g.V().sample(5).next(); // sample can't be predicate
            //g.V().coin(0.5).next();
            //g.V().limit(5).next();
            //g.V().range(1, 5).next();
            //g.V().tail(5).next();
            //g.V().@where("name", Predicate.eq("jinjin")).next(); //pass
            //g.V().@where("age", Predicate.neq(1)).next(); //pass
            //g.V().@where("name", Predicate.lt(1)).next(); //pass
            //g.V().@where("name", Predicate.gt(1)).next(); //pass
            //g.V().@where("name", Predicate.gte(1)).next();
            //g.V().next();

            //g.V().and(GraphTraversal2.underscore().outE("knows"),
            //            GraphTraversal2.underscore().@where("name", Predicate.eq("jinjin"))).values("name").next(); //pass
            //g.V().Or(GraphTraversal2.underscore().outE("knows"),
            //            GraphTraversal2.underscore().@where("name", Predicate.eq("jinjin"))).values("name").next(); //pass
            //g.V().and(GraphTraversal2.underscore().outE("knows"),
            //            GraphTraversal2.underscore().values("age").Is(Predicate.lt(30))).values("name").next();

            //g.E().next();
            /*
                sqlFragment: WSqlFragment
                    FirstTokenIndex: int
                    FromClause: WFromClause
                    GroupByClause: WGroupByClause
                    HavingClause: WHavingClause
                    Into: WSchemaObjectName
                    LastTokenIndex: int
                    LimitClause: WLimitClause
                    MatchClause: WMatchClause
                    OrderByClause: WOrderClause
                    OutputPath: bool
                    SelectElements: List<WSelectElement>
                    TopRowFilter: WTopRowFilter
                    UniqueRowFilter: UniqueRowFilter
                    WhereClause: WWhereClause
                    WithPathClause: WWithPathClause
             */

            //g.V().addE("123").next();
            /*
                 sqlFragment: WInsertEdgeSpecification
                    Columns: List<WColumnReferenceExpression>
                    EdgeColumn: WColumnReferenceExpression
                    FirstTokenIndex: int
                    InsertOption: InsertOption
                    InsertSource: WInsertSource
                    LastTokenIndex: int
                    SelectInsertSource: WSelectInsertSource
                    Target: WNamedTableReference
                    TopRowFilter: WTopRowFilter
             
             */
        }

        [TestMethod]
        public void test()
        {
            const string q2 = @"SELECT 1, n_0.id FROM node n_0, node_n1 cross apply tvf(n_0, n_1) as n2 Group BY name";

            var sr = new StringReader(q2);
            var parser = new GraphViewParser();
            IList<ParseError> errors;
            var script = parser.Parse(sr, out errors) as WSqlScript;
            if (errors.Count > 0)
                throw new SyntaxErrorException(errors);
        }

    }
}