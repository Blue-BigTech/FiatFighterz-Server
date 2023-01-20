using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.NET.Logging;
using unirest;
using Newtonsoft.Json.Linq;
using Utils.NET.Net.Web;
using System.Threading;
using Utils.NET.Net.RateLimiting;
using System.Net;
using System.Collections.Specialized;
using TitanCore.Net.Web;
using Amazon.DynamoDBv2.Model;
using TitanDatabase;
using TitanDatabase.Models;
using TitanCore.Data;

namespace TitanDatabase
{
    // https://docs.moralis.io/moralis-server/web3-sdk/intro
    public class BlockChainClient
    {

        private List<WebGetOwnedNFTsInfo> NFTS;
        //private WebListener listener;

        string chain = "mumbai";
        string network = "testnet";
        string contract = "0x46ad2312900BacAEbfCcb5274662e59c73F79647";
        
        public DateTime chainLastUpdate;
        public Dictionary<string, string> CryptoNFTmap;



        public BlockChainClient()
        {
            //listener = new WebListener(Prefix);
            InitChain();
        }

        private static string ParseTokenValues(Dictionary<string, AttributeValue> attributeList)
        {
            string token_id = "";
            foreach (KeyValuePair<string, AttributeValue> kvp in attributeList)
            {
                string attributeName = kvp.Key;
                AttributeValue value = kvp.Value;
                token_id += (value.S);
            }
            return token_id;
        }

        public async void InitChain()
        {
            // this sends a request to moralis and then parses the data it gets back
            // checks for new nfts etc
            
            // need a way to logout accounts that no longer own a token they are playing on
            var request = new ScanRequest
            {
                TableName = Database.Table_Characters,
                ProjectionExpression = "token_id"
            };
            var res = await Database.Client.ScanAsync(request);
            List<string> database_tokens = new List<string>();
            List<string> blockchain_tokens = new List<string>();
            List<int> tokenindex = new List<int>();
            foreach (Dictionary<string, AttributeValue> item in res.Items)
            {
                string value = ParseTokenValues(item);
                database_tokens.Add(value);
            }
            Log.Write(res);

            NFTS = new List<WebGetOwnedNFTsInfo>();
            
            HttpResponse<string> response = Unirest.get(String.Format("https://deep-index.moralis.io/api/v2/nft/{0}/owners?chain={1}&format=decimal", contract, chain))
              .header("accept", "application/json")
              .header("X-API-Key", "E2YICuMIR7b2kNwaVXLJMyCqaUQL0sdM1VwtkRMtwunaNzMBWoIjxxVIVhfNP6oD")  // Moralis Web3 API
              .asJson<string>();
            JObject jsonResponse = JObject.Parse(response.Body);
            Dictionary<string, WebGetOwnedNFTsInfo> valmap = new Dictionary<string, WebGetOwnedNFTsInfo>();
            int x = 1;
            foreach (var jsin in jsonResponse["result"])
            {
                blockchain_tokens.Add(jsin["token_id"].ToString());
                valmap.Add(x.ToString(), new WebGetOwnedNFTsInfo(jsin["token_id"].ToString(), jsin["owner_of"].ToString(), jsin["metadata"].ToString()));
                NFTS.Add(new WebGetOwnedNFTsInfo(jsin["token_id"].ToString(), jsin["owner_of"].ToString(), jsin["metadata"].ToString()));
                x++;
            }


            Log.Write(database_tokens.Count + " " + blockchain_tokens.Count);

            // valid_tokens vs NFTS find newly minted characters and create a character for them
            /*
            for( int i = 0; i < database_tokens.Count; i++)
            {
                Console.WriteLine(blockchain_tokens[i] + " " + database_tokens[i]);
            }*/
            var tokens_to_add = blockchain_tokens.Except(database_tokens);
            foreach (var token in tokens_to_add)
            {
                if (valmap.TryGetValue(token.ToString(), out WebGetOwnedNFTsInfo value))
                {
                    Log.Write("CHARACTER CREATED: " + value.token_id);
                    CreateCharacter(value);
                }else
                {
                    Console.WriteLine(token.ToString() + " not found" );
                }
            }
            // get the nfts on characters from database
        }

        /// <summary>
        /// used for generating the table id for the database
        /// this value needs to be able to be derived from the nft so to avoid multiple stages
        /// of database searches
        /// </summary>
        /// <param name="token_id"></param>
        /// <returns></returns>
        public ulong GetDatabaseId(string token_id)
        {
            string splitcontract = contract.Substring(0,6);
            ulong id = ulong.Parse(Convert.ToInt32(splitcontract, 16).ToString());
            id += ulong.Parse(token_id);
            return id;
        }

        public async void CreateCharacter(WebGetOwnedNFTsInfo nftsInfo)
        {
            //Console.WriteLine("nfts info "+nftsInfo.token_id);
            var x = await Database.CreateCharacterNFT(nftsInfo, GetDatabaseId(nftsInfo.token_id));
        }


        bool running = true;
        public bool updatingChain = false;


        // needs to be moved into an async funciton
        public async Task UpdateChain()
        {


            // need a way to logout accounts that no longer own a token they are playing on

            // http response to moralis api
            updatingChain = true;


            NFTS = new List<WebGetOwnedNFTsInfo>();

            HttpResponse<string> response = Unirest.get(String.Format("https://deep-index.moralis.io/api/v2/nft/{0}/owners?chain={1}&format=decimal", contract, chain))
              .header("accept", "application/json")
              .header("X-API-Key", "E2YICuMIR7b2kNwaVXLJMyCqaUQL0sdM1VwtkRMtwunaNzMBWoIjxxVIVhfNP6oD")  // Moralis Web3 API
              .asJson<string>();
            JObject jsonResponse = JObject.Parse(response.Body);
            foreach (var jsin in jsonResponse["result"])
            {
                /*  EXAMPLE RESPONSE 
                   {
                   "token_address": "0x26f0bf1703b27cad96d22775e10a8e80d3f393dc",
                   "token_id": "23",
                   "block_number_minted": "25294274",
                   "owner_of": "0xf4fe12675f612c1cb2e47e7dcc79d5149ac33eb9",
                   "block_number": "25294274",
                   "amount": "1",
                   "contract_type": "ERC721",
                   "name": "FIATV3",
                   "symbol": "FIATV3",
                   "token_uri": "https://ipfs.moralis.io:2053/ipfs/QmZPa4wckUZiFuFC5YZbGQBGKWThtSuFrHcLvihh8VBQJx/23.json",
                   "metadata": "{\"name\":\"FIATFIGHTERZ #23\",\"description\":\"To play with this NFT please visit FiatFighterz.io and login!\",\"image\":\"ipfs://QmcxAbSC1oWU5M2VuFvdYYdUPU9umEWUnwatFVvtSQ1Msq/23.png\",\"edition\":23,\"date\":1645910912130,\"attributes\":[{\"trait_type\":\"Background\",\"value\":\"Green\"},{\"trait_type\":\"Class\",\"value\":\"Wizard\"},{\"trait_type\":\"Race\",\"value\":\"Human\"},{\"trait_type\":\"DNA\",\"value\":\"f91801197e25f09a7016d4145159dea17215d348\"},{\"trait_type\":\"Character\",\"value\":\"Wizard_Human\"}]}",
                   "synced_at": "2022-02-26T21:40:50.846Z",
                   "is_valid": 1,
                   "syncing": 2,
                   "frozen": 0
                   }
                */

                NFTS.Add(new WebGetOwnedNFTsInfo(jsin["token_id"].ToString(), jsin["owner_of"].ToString(), jsin["metadata"].ToString()));
            }
            updatingChain = false;
            chainLastUpdate = DateTime.UtcNow;

            Log.Write("Loading BlockChain Client...");
        }

        // how fast the chain gets called to update
        float UpdateSpeed = 120;

        // called from the webserver to get the nfts owned by this wallet
        public async Task<WebGetOwnedNFTsResponse> DetermineOwnership(string accountId, string cryptoAddress)
        {
            // chain is updated 
            if ((DateTime.UtcNow - chainLastUpdate).TotalSeconds > UpdateSpeed)
            {
                await UpdateChain();
                Log.Write("Updated from blockchain");
            }

            WebGetOwnedNFTsResponse response = new WebGetOwnedNFTsResponse();
            List<WebGetOwnedNFTsInfo> ownednfts = new List<WebGetOwnedNFTsInfo>();

            if (string.IsNullOrWhiteSpace(accountId) && string.IsNullOrWhiteSpace(accountId))
            {
                return new WebGetOwnedNFTsResponse(WebGetOwnedNFTsResult.InvalidRequest);
            }
            foreach (WebGetOwnedNFTsInfo nft in NFTS)
            {
                if (cryptoAddress.ToLower().Equals(nft.owner_of.ToLower()))
                {
                    ownednfts.Add(nft);
                }else
                {
                    //ownednfts.Add(nft); // Jinge testing
                    Console.WriteLine(nft.owner_of + "  cryptoAddress: " + cryptoAddress);
                }
            }

            if (ownednfts.Count > 0)
            {
                response.result = WebGetOwnedNFTsResult.Success;
                response.OwnedNfts = ownednfts.ToArray();
                return response;
            }
            else
            {
                response.result = WebGetOwnedNFTsResult.InvalidRequest;
                response.OwnedNfts = new WebGetOwnedNFTsInfo[0];
                return response;
            }
        }

        public async Task<WebSellItemResponse> DetermineItemOwnership(string accountId, string characterId, string itemId)
        {
            //await Task.Delay(5000);
            if (string.IsNullOrWhiteSpace(accountId) && string.IsNullOrWhiteSpace(characterId) && string.IsNullOrWhiteSpace(itemId))
            {
                return new WebSellItemResponse(WebSellItemResult.InvalidRequest);
            }
            CryptoNFTmap = new Dictionary<string, string>();


            // grabs crypto address out of the database associated with that account id
            var request = new ScanRequest
            {
                TableName = Database.Table_Accounts,
                FilterExpression = "id = :id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":id", new AttributeValue { N = accountId } }
                },
                ProjectionExpression = "cryptoAddress"
            };
            var res = await Database.Client.ScanAsync(request);
            string cryptoAddress="";

            
            if (res.Items.Count != 1)
            {
                if (res.Items.Count > 1)
                {
                    Console.WriteLine("Duplicate Account Id found on: ");
                    foreach (Dictionary<string, AttributeValue> row in res.Items)
                    {
                        Console.WriteLine(row["cryptoAddress"].S);
                    }
                }else
                {
                    Console.WriteLine("Account Id: " + accountId + " Does Not Exist");
                }
                
                return new WebSellItemResponse(WebSellItemResult.InvalidRequest);
            }
            else
            {
                cryptoAddress = res.Items[0]["cryptoAddress"].S;

            }

            request = new ScanRequest
            {
                TableName = Database.Table_Characters,
                FilterExpression = "id = :id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":id", new AttributeValue { N = characterId } }
                },
                ProjectionExpression = "token_id, itemIds"
            };
            res = await Database.Client.ScanAsync(request);


            string token_id = "";
            if (res.Items.Count != 1)
            {
                if (res.Items.Count > 1)
                {
                    Console.WriteLine("Duplicate Character Id found on: ");
                    foreach (Dictionary<string, AttributeValue> row in res.Items)
                    {
                        Console.WriteLine(row["token_id"].S);
                    }
                }
                else
                {
                    Console.WriteLine("Account Id: " + characterId + " Does Not Exist");
                }

                return new WebSellItemResponse(WebSellItemResult.InvalidRequest);
            }
            else
            {
                token_id = res.Items[0]["token_id"].S;
            }
            List<string> items = res.Items[0]["itemIds"].NS;

            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine(items.Count);
            Console.WriteLine(NFTS.Count);

            foreach (WebGetOwnedNFTsInfo nft in NFTS)
            {
                if (nft.owner_of.ToUpper() == cryptoAddress.ToUpper() && nft.token_id == token_id)
                {
                    Console.WriteLine("Ownership Determined: " + nft.owner_of + " " + nft.token_id);
                }
            }



            Console.WriteLine("crypto address: " + cryptoAddress);
            Console.WriteLine("token id: " + token_id);



            WebSellItemResponse response = new WebSellItemResponse();
            List<WebGetOwnedNFTsInfo> ownednfts = new List<WebGetOwnedNFTsInfo>();


            return new WebSellItemResponse(WebSellItemResult.InvalidRequest);
        }



    }
}





