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
    public class ExploreTvShowsModel : PageModel
    {
        [BindProperty]
        public SelectList zanrovi {get; set;}
       
        [BindProperty(SupportsGet=true)]
        public string zanrFilter {get; set;}
        [BindProperty(SupportsGet=true)]
        public string godinaFilter {get; set;}
        [BindProperty(SupportsGet=true)]
        public string ocenaOdFilter {get; set;}
        [BindProperty(SupportsGet=true)]
        public string ocenaDoFilter {get; set;}
        [BindProperty(SupportsGet=true)]
        public string brojSezonaOdFilter {get; set;}
        [BindProperty(SupportsGet=true)]
        public string brojSezonaDoFilter {get; set;}
        [BindProperty(SupportsGet=true)]
        public string nazivFilter {get; set;}
        [BindProperty]
        public List<Serija> serije {get; set;}
        public string Message {get; set;}
        public void OnGet()
        {
            Neo4jClient.GraphClient client = ClientManager.GetSession();

            string email = HttpContext.Session.GetString("email");
            if(!String.IsNullOrEmpty(email))
                Message = "Welcome " + email;

            var query = new Neo4jClient.Cypher.CypherQuery("start n=node(*) match(n:Serija) where exists(n.zanr) return distinct n.zanr",
                                                           new Dictionary<string, object>(), CypherResultMode.Set);

            List<string> listaZanrova = ((IRawGraphClient)client).ExecuteGetCypherResults<string>(query).ToList();
            zanrovi = new SelectList(listaZanrova);  
            
            String queryString;

            if(!String.IsNullOrEmpty(nazivFilter))
            {
                queryString = "start n=node(*) match(n:Serija) where n.nazivSerije =~ '.*(?i)" + nazivFilter + ".*'  return n order by n.zanr";
            }
            else queryString = setQueryString();

            var query2 = new Neo4jClient.Cypher.CypherQuery(queryString,
                                                           new Dictionary<string, object>(), CypherResultMode.Set);

            serije = ((IRawGraphClient)client).ExecuteGetCypherResults<Serija>(query2).ToList();
        }

        public String setQueryString()
        {
            bool prvi = true;

            String queryFilter = "start n=node(*) match(n:Serija) ";
            if(!String.IsNullOrEmpty(zanrFilter))
            {
                if(prvi)
                {
                    queryFilter += "where n.zanr = '" + zanrFilter + "'";
                    prvi = false;
                }
                else queryFilter += " and n.zanr = '" + zanrFilter + "'";
            }

            /*if(!String.IsNullOrEmpty(godinaFilter))
            {
                if(prvi)
                {
                    queryFilter += "where n.godina = " + godinaFilter;
                    prvi = false;
                }
                else queryFilter += " and n.godina = " + godinaFilter;
            }*/
            
            if(!String.IsNullOrEmpty(ocenaOdFilter))
            {
                if(prvi)
                {
                    queryFilter += "where n.avgOcena >= " + ocenaOdFilter;
                    prvi = false;
                }
                else queryFilter += " and n.avgOcena >= " + ocenaOdFilter;
            }

            if(!String.IsNullOrEmpty(ocenaDoFilter))
            {
                if(prvi)
                {
                    queryFilter += "where n.avgOcena <= " + ocenaDoFilter;
                    prvi = false;
                }
                else queryFilter += " and n.avgOcena <= " + ocenaDoFilter;
            }

            queryFilter += " return n order by n.zanr";
            return queryFilter;
        }
    }
}
