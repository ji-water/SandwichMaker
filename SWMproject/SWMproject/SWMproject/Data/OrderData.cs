﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWMproject.Data
{
    public class OrderData
    {
        public int price { get; set; }

        //2. 메뉴 선택
        public string Menu { get; set; }
        //3. 빵 선택
        public string Bread { get; set; }
        //4. 치즈 선택
        public string Cheese { get; set; }
        //5. 데우기
        public bool Warmup { get; set; }
        //6. 빼는 야채
        public string[] Vege { get; set; } = { "토마토", "올리브","양상추","양파","파프리카","오이","피망","피클","할라피뇨" };
        //7. 소스 선택
        public string Sauce { get; set; }
        //8. 세트 선택
        public string SetMenu { get; set; }
        public string SetDrink { get; set; }
        //단품
        public string[] AddiOrder { get; set; }
        //9. 요구사항
        public string Requirement { get; set; }
    }
}
