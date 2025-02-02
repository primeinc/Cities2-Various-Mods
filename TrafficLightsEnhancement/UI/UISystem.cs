using System;
using System.Collections;
using System.Collections.Generic;
using cohtml.Net;
using Game;
using Game.SceneFlow;
using Newtonsoft.Json;

namespace TrafficLightsEnhancement.UI;

public class UISystem : GameSystemBase
{
    public struct MenuItemDivider {
        [JsonProperty]
        const string itemType = "divider";
    }

    public struct MenuItemPattern {
        [JsonProperty]
        const string itemType = "radio";

        [JsonProperty]
        const string type = "c2vm-tle-panel-pattern";

        public int ways;

        public int pattern;

        public string label;
    }

    public struct MenuItemTitle {
        [JsonProperty]
        const string itemType = "title";

        public string title;
    }

    public struct MenuItemOption {
        [JsonProperty]
        const string itemType = "checkbox";

        [JsonProperty]
        const string type = "c2vm-tle-panel-option";

        public string key;

        public string value;

        public string label;
    }

    public Dictionary<string, int> m_Options;

    public int[] m_SelectedPattern;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_Options = new Dictionary<string, int>();
        m_SelectedPattern = new int[16];

        View view = GameManager.instance.userInterface.view.View;
        view.BindCall("C2VM-TLE-RequestMenuData", RequestMenuData);
        view.BindCall("C2VM-TLE-OptionChanged", OptionChanged);
        view.BindCall("C2VM-TLE-PatternChanged", PatternChanged);
        view.ExecuteScript("""
            if(!document.querySelector("div.c2vm-tle-panel")){const e=document.createElement("div");e.innerHTML='\n        <div class="c2vm-tle-panel">\n            <div class="c2vm-tle-panel-header">\n                <img class="c2vm-tle-panel-header-image" src="Media/Game/Icons/TrafficLights.svg" />\n                <div class="c2vm-tle-panel-header-title">Traffic Lights Enhancement</div>\n            </div>\n            <div class="c2vm-tle-panel-content"></div>\n        </div>\n        <style>\n            .c2vm-tle-panel {\n                width: 300rem;\n                position: absolute;\n                top: calc(10rem+ var(--floatingToggleSize) +6rem);\n                left: 10rem;\n            }\n            .c2vm-tle-panel-header {\n                border-radius: 4rem 4rem 0rem 0rem;\n                background-color: rgba(24, 33, 51, 0.6);\n                backdrop-filter: blur(5px);\n                color: rgba(75, 195, 241, 1);\n                font-size: 14rem;\n                padding: 6rem 10rem;\n                min-height: 36rem;\n                display: flex;\n                flex-direction: row;\n                align-items: center;\n            }\n            .c2vm-tle-panel-header-image {\n                width: 24rem;\n                height: 24rem;\n            }\n            .c2vm-tle-panel-header > .c2vm-tle-panel-header-title {\n                text-transform: uppercase;\n                flex: 1;\n                text-align: center;\n                overflow-x: hidden;\n                overflow-y: hidden;\n                text-overflow: ellipsis;\n                white-space: nowrap;\n            }\n            .c2vm-tle-panel-content {\n                border-radius: 0rem 0rem 4rem 4rem;\n                background-color: rgba(42, 55, 83, 0.437500);\n                backdrop-filter: blur(5px);\n                color: rgba(255, 255, 255, 1);\n                flex: 1;\n                position: relative;\n                padding: 6rem;\n            }\n            .c2vm-tle-panel-row {\n                padding: 3rem 8rem;\n                width: 100%;\n                display: flex;\n            }\n            .c2vm-tle-panel-row-divider {\n                height: 2px;\n                width: auto;\n                border: 2px solid rgba(255, 255, 255, 0.1);\n                margin: 6rem -6rem;\n            }\n            .c2vm-tle-panel-secondary-text {\n                color: rgba(217, 217, 217, 1);\n            }\n            .c2vm-tle-panel-radio {\n                border: 2px solid rgba(75, 195, 241, 1);\n                margin: 0 10rem 0 0;\n                width: 20rem;\n                height: 20rem;\n                padding: 3px;\n                border-radius: 50%;\n            }\n            .c2vm-tle-panel-radio-bullet {\n                width: 100%;\n                height: 100%;\n                background-color:  white;\n                opacity: 0;\n                border-radius: 50%;\n            }\n            .c2vm-tle-panel-radio-bullet-checked {\n                opacity: 1;\n            }\n            .c2vm-tle-panel-checkbox {\n                margin: 0 10rem 0 0;\n                width: 20rem;\n                height: 20rem;\n                padding: 1px;\n                border: 2px solid rgba(255, 255, 255, 0.500000);\n                border-radius: 3rem;\n            }\n            .c2vm-tle-panel-checkbox-checkmark {\n                width: 100%;\n                height: 100%;\n                mask-image: url(Media/Glyphs/Checkmark.svg);\n                mask-size: 100% auto;\n                background-color: white;\n                opacity: 0;\n            }\n            .c2vm-tle-panel-checkbox-checkmark-checked {\n                opacity: 1;\n            }\n        </style>\n    ';document.querySelector("body").appendChild(e);const n=()=>{const e=document.querySelectorAll('[data-type="c2vm-tle-panel-pattern"] .c2vm-tle-panel-radio-bullet');for(const n of e)n.classList.remove("c2vm-tle-panel-radio-bullet-checked")},t=e=>{n();const t=JSON.parse(e);console.log(`C2VM-TLE-PatternChanged result ${e} m_SelectedPattern ${t}`);for(const e in t){const n=e,a=65535&t[n],l=document.querySelector(`[data-type="c2vm-tle-panel-pattern"][data-ways="${n}"][data-pattern="${a}"] .c2vm-tle-panel-radio-bullet`);l&&l.classList.add("c2vm-tle-panel-radio-bullet-checked")}},a=e=>{n();const a=e.currentTarget.dataset;console.log(e.currentTarget,a,a.ways,a.pattern),engine.call("C2VM-TLE-PatternChanged",`${a.ways}_${a.pattern}`).then(t)},l=()=>{const e=document.querySelectorAll('[data-type="c2vm-tle-panel-option"] .c2vm-tle-panel-checkbox-checkmark');for(const n of e)n.classList.remove("c2vm-tle-panel-checkbox-checkmark-checked")},c=e=>{l();const n=JSON.parse(e);console.log(`C2VM-TLE-OptionChanged result ${e} m_Options ${n}`);for(const e in n){const t=n[e],a=document.querySelector(`[data-type="c2vm-tle-panel-option"][data-key="${e}"]`);a.dataset.value=t;const l=a.querySelector(".c2vm-tle-panel-checkbox-checkmark");l&&1===t&&l.classList.add("c2vm-tle-panel-checkbox-checkmark-checked")}},r=e=>{l();const n=e.currentTarget.dataset;console.log(e.currentTarget,n);let t=0;0==n.value&&(t=1),engine.call("C2VM-TLE-OptionChanged",`${n.key}_${t}`).then(c)};engine.call("C2VM-TLE-RequestMenuData").then((e=>{const n=JSON.parse(e),l=document.querySelector(".c2vm-tle-panel-content");console.log(n,l);for(const e of n)if(e.itemType){if("divider"==e.itemType){const e=document.createElement("div");e.classList.add("c2vm-tle-panel-row-divider"),l.appendChild(e)}if("title"==e.itemType){const n=document.createElement("div");n.classList.add("c2vm-tle-panel-row"),n.innerHTML=e.title,l.appendChild(n)}if("radio"==e.itemType){const n=document.createElement("div");n.classList.add("c2vm-tle-panel-row");for(const t in e)n.dataset[t]=e[t];n.innerHTML+=`\n                    <div class="c2vm-tle-panel-radio">\n                        <div class="c2vm-tle-panel-radio-bullet"></div>\n                    </div>\n                    <span class="c2vm-tle-panel-secondary-text">${e.label}</span>\n                `,l.appendChild(n)}if("checkbox"==e.itemType){const n=document.createElement("div");n.classList.add("c2vm-tle-panel-row");for(const t in e)n.dataset[t]=e[t];n.innerHTML+=`\n                    <div class="c2vm-tle-panel-checkbox">\n                        <div class="c2vm-tle-panel-checkbox-checkmark"></div>\n                    </div>\n                    <span class="c2vm-tle-panel-secondary-text">${e.label}</span>\n                `,l.appendChild(n)}}const o=document.querySelectorAll('[data-type="c2vm-tle-panel-option"]');for(const e of o)e.onclick=r;const i=document.querySelectorAll('[data-type="c2vm-tle-panel-pattern"]');for(const e of i)e.dataset.ways>0&&(e.onclick=a);engine.call("C2VM-TLE-PatternChanged","3_0").then(t),engine.call("C2VM-TLE-PatternChanged","4_0").then(t),engine.call("C2VM-TLE-OptionChanged","ExclusivePedestrian_1").then(c),engine.call("C2VM-TLE-OptionChanged","AlwaysGreenKerbsideTurn_0").then(c)}));const o=document.querySelector("body"),i={attributes:!0,childList:!0,subtree:!0};new MutationObserver(((e,n)=>{const t=document.querySelector("button.selected.item_KJ3.item-hover_WK8.item-active_Spn > img"),a=document.querySelector("div.c2vm-tle-panel");a&&(a.style.display=t&&"Media/Game/Icons/TrafficLights.svg"==t.src?"block":"none")})).observe(o,i)}
        """);
    }

    protected override void OnUpdate()
    {
    }

    protected string RequestMenuData()
    {
        var menu = new ArrayList()
        {
            new MenuItemTitle{title = "Three-Way Junction"},
            new MenuItemPattern{label = "Vanilla", ways = 3, pattern = (int) PatchedClasses.TrafficLightPatterns.Pattern.Vanilla},
            new MenuItemPattern{label = "Split Phasing", ways = 3, pattern = (int) PatchedClasses.TrafficLightPatterns.Pattern.SplitPhasing},
            new MenuItemTitle{title = "Four-Way Junction"},
            new MenuItemPattern{label = "Vanilla", ways = 4, pattern = (int) PatchedClasses.TrafficLightPatterns.Pattern.Vanilla},
            new MenuItemPattern{label = "Split Phasing", ways = 4, pattern = (int) PatchedClasses.TrafficLightPatterns.Pattern.SplitPhasing},
            new MenuItemPattern{label = "Advanced Split Phasing", ways = 4, pattern = (int) PatchedClasses.TrafficLightPatterns.Pattern.SplitPhasingAdvanced},
            new MenuItemPattern{label = "Protected Centre-Turn", ways = 4, pattern = (int) PatchedClasses.TrafficLightPatterns.Pattern.ProtectedCentreTurn},
            default(MenuItemDivider),
            new MenuItemTitle{title = "Options"},
            new MenuItemOption{label = "Exclusive Pedestrian Phase", key = PatchedClasses.TrafficLightPatterns.Pattern.ExclusivePedestrian.ToString(), value = "1"},
            new MenuItemOption{label = "Always Green Kerbside-Turn", key = PatchedClasses.TrafficLightPatterns.Pattern.AlwaysGreenKerbsideTurn.ToString(), value = "0"}
        };
        string result = JsonConvert.SerializeObject(menu);
        return result;
    }

    protected string OptionChanged(string input)
    {
        string[] splitInput = input.Split('_');
        if (splitInput.Length == 2)
        {
            string key = splitInput[0];
            int.TryParse(splitInput[1], out int value);
            if (value != 1)
            {
                value = 0;
            }
            m_Options[key] = value;
        }
        UpdatePatterns();
        string result = JsonConvert.SerializeObject(m_Options);
        Console.WriteLine($"UISystem OptionChanged {input} {result}");
        return result;
    }

    protected string PatternChanged(string input)
    {
        string[] splitInput = input.Split('_');
        if (splitInput.Length == 2)
        {
            int ways;
            int pattern;
            int.TryParse(splitInput[0], out ways);
            int.TryParse(splitInput[1], out pattern);
            if (ways < m_SelectedPattern.Length)
            {
                if (!PatchedClasses.TrafficLightPatterns.IsValidPattern(ways, pattern))
                {
                    pattern = 0;
                }
                m_SelectedPattern[ways] = pattern;
            }
        }
        UpdatePatterns();
        string result = JsonConvert.SerializeObject(m_SelectedPattern);
        Console.WriteLine($"UISystem PatternChanged {input} {result}");
        return result;
    }

    protected void UpdatePatterns()
    {
        for (int i = 0; i < m_SelectedPattern.Length; i++)
        {
            if (
                m_Options.ContainsKey(PatchedClasses.TrafficLightPatterns.Pattern.ExclusivePedestrian.ToString()) &&
                m_Options[PatchedClasses.TrafficLightPatterns.Pattern.ExclusivePedestrian.ToString()] == 1
            )
            {
                m_SelectedPattern[i] = m_SelectedPattern[i] | (int) PatchedClasses.TrafficLightPatterns.Pattern.ExclusivePedestrian;
            }
            else
            {
                m_SelectedPattern[i] = m_SelectedPattern[i] & (int) ~PatchedClasses.TrafficLightPatterns.Pattern.ExclusivePedestrian;
            }

            if (
                m_Options.ContainsKey(PatchedClasses.TrafficLightPatterns.Pattern.AlwaysGreenKerbsideTurn.ToString()) &&
                m_Options[PatchedClasses.TrafficLightPatterns.Pattern.AlwaysGreenKerbsideTurn.ToString()] == 1
            )
            {
                m_SelectedPattern[i] = m_SelectedPattern[i] | (int) PatchedClasses.TrafficLightPatterns.Pattern.AlwaysGreenKerbsideTurn;
            }
            else
            {
                m_SelectedPattern[i] = m_SelectedPattern[i] & (int) ~PatchedClasses.TrafficLightPatterns.Pattern.AlwaysGreenKerbsideTurn;
            }
        }
    }
}