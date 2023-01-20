using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data.Entities;
using Utils.NET.IO;
using Utils.NET.Logging;
namespace TitanDatabase.Models
{
	public class TokenTransaction : Model
	{

        public override string TableName => Database.Table_Token_Transactions;

        public ushort id;
		public string token_id;
		public float transaction_ammount;
		public ushort item_id;

        public override void Read(ItemReader r)
        {
            throw new NotImplementedException();
        }

        public override void Write(ItemWriter w)
        {
            throw new NotImplementedException();
        }

        public static async Task<GetResponse<TokenTransaction>> Get(ulong id)
        {
            var request = new GetItemRequest(Database.Table_Accounts, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } }, true);
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<TokenTransaction>
                {
                    result = response.result,
                };

            var account = new Account();
            account.Read(new ItemReader(response.item));

            var itemLoadResponse = await Database.LoadItems(account.vaultIds);
            switch (itemLoadResponse.result)
            {
                case LoadItemsResult.AwsError:
                    return new GetResponse<TokenTransaction>
                    {
                        result = RequestResult.InternalServerError,
                        item = null
                    };
                case LoadItemsResult.Success:
                    account.vaultItems = itemLoadResponse.items;
                    break;
            }

            return new GetResponse<TokenTransaction>
            {
                result = RequestResult.Success,
                //item = account
            };
        }

        public TokenTransaction()
		{


		}

        public override bool IsDifferent()
        {
            return true;
        }
    }


}

