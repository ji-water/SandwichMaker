﻿using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using SWMproject.Data;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace SWMproject.Dialogs
{
    public class LocationDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<OrderData> _orderDataAccessor;
        
        public LocationDialog(UserState userState) : base(nameof(LocationDialog))
        {
            _orderDataAccessor = userState.CreateProperty<OrderData>("OrderData");
            //실행 순서
            var waterfallSteps = new WaterfallStep[]
            {
                UserInputStepAsync,
                FindShopStepAsync,
            };
            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> UserInputStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var msg = "주문 할 서브웨이 지점을 선택하세요! \r\n서브웨이를 이용할 주변 역이나 주소를 입력해주세요 (예: 이대역, 서대문구, 아현동)";
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text(msg) };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> FindShopStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string Input = (string)stepContext.Result;

            string site = "https://dapi.kakao.com/v2/local/search/keyword.json";
            string query = string.Format("{0}?query={1}", site, Input+" 서브웨이");
            WebRequest request = WebRequest.Create(query);
            string rkey = "bae3f470ce71b9925187548a1306e592";

            string header = "KakaoAK " + rkey;
            request.Headers.Add("Authorization", header);

            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);

            string json = reader.ReadToEnd();

            stream.Close();

            JObject jtemp = JObject.Parse(json);
            JArray array = JArray.Parse(jtemp["documents"].ToString());

            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            foreach (JObject jobj in array)
            {
                string place_name = jobj["place_name"].ToString();
                string phone = jobj["phone"].ToString();
                string address_name = jobj["address_name"].ToString();

                string x = jobj["x"].ToString();
                string y = jobj["y"].ToString();
                string kakaoUrl = "https://map.kakao.com/link/to/"+place_name+","+ x+","+y;

                var heroCard = new HeroCard
                {
                    Title = place_name,
                    Subtitle = phone,
                    Text = address_name,
                    Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.OpenUrl,value: kakaoUrl,title:"카카오맵에서 확인하기"),
                        new CardAction(ActionTypes.ImBack, "여기서 주문하기", value: "선택")
                    },
                };
                reply.Attachments.Add(heroCard.ToAttachment());
            }
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.BeginDialogAsync(nameof(OrderDialog), null, cancellationToken);
        }

    }
}
