using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neo4j.Model;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace MyApp.Namespace
{
    public class SeasonModel : PageModel
    {
        [BindProperty]
        public Serija serija {get;set;}
        [BindProperty]
        public Sezona sezona {get;set;}
        [BindProperty]
        public List<Epizoda> epizode {get;set;}
        public string Message {get; set;}

        public void OnGet(string tvShow, string season)
        {
            Neo4jClient.GraphClient client = ClientManager.GetSession();
            string email = HttpContext.Session.GetString("email");
            if(!String.IsNullOrEmpty(email))
            {
                Dictionary<string, object> queryDict0 = new Dictionary<string, object>();
                queryDict0.Add("email", email);

                var query0 = new Neo4jClient.Cypher.CypherQuery("start n=node(*) match(k:Korisnik) where k.email = {email} return k",
                                                           queryDict0, CypherResultMode.Set);

                Korisnik k = ((IRawGraphClient)client).ExecuteGetCypherResults<Korisnik>(query0).FirstOrDefault();
                if(k.tip == 1)
                    Message = "Admin";
                else Message = "User";
            }

            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            queryDict.Add("season", season);
            queryDict.Add("tvShow", tvShow);
            
            
            var query = new Neo4jClient.Cypher.CypherQuery("start n=node(*) match (serija)-[:SEASON]->(sezona) where sezona.nazivSezone = {season} and serija.nazivSerije = {tvShow} return sezona",
                                                           queryDict, CypherResultMode.Set);

            sezona = ((IRawGraphClient)client).ExecuteGetCypherResults<Sezona>(query).FirstOrDefault();

            var query2 = new Neo4jClient.Cypher.CypherQuery("start n=node(*) match(n:Serija) where n.nazivSerije = {tvShow} return n",
                                                           queryDict, CypherResultMode.Set);

            serija = ((IRawGraphClient)client).ExecuteGetCypherResults<Serija>(query2).FirstOrDefault();

            var query3 = new Neo4jClient.Cypher.CypherQuery("match (sezona)-[r:EPISODE]->(epizoda) where sezona.nazivSezone = {season} return epizoda",
                                                           queryDict, CypherResultMode.Set);

            epizode = ((IRawGraphClient)client).ExecuteGetCypherResults<Epizoda>(query3).OrderBy(x=>x.datumObjave).ToList();
        }
    }
}
