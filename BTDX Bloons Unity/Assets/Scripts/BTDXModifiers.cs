using UnityEngine;

#if !(UNITY_EDITOR || UNITY_STANDALONE)
using Il2CppAssets.Scripts.Unity.UI_New.InGame.BloonMenu;
using MelonLoader;
using UnityEngine.UI;
using Resources = BTDXBloons.Properties.Resources;
#endif

namespace BTDXBloons.MonoBehaviors {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
    [RegisterTypeInIl2Cpp(logSuccess: false)]
#endif
    public class BTDXModifiers : MonoBehaviour {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
        private const float openMovement = 420;
        private float openX, closedX;
        private float popoutBtnOffset;
        private float t = 1;

        private static readonly Vector2 onSize = new(182, 182);
        private static readonly Vector2 offSize = new(170, 170);

        private BloonMenu BloonMenu;

        private Button PopoutBtn;
        private Image PopoutImg;
        private bool Open = false;
        private bool WasOpen = false;
        private bool WasMenuShowing;

        private static Sprite ShieldedOnIcon => Resources.GetResource<Sprite>("BloonTypeShieldedOn");
        private static Sprite ShieldedOffIcon => Resources.GetResource<Sprite>("BloonTypeShieldedOff");
        private static Sprite StaticOnIcon => Resources.GetResource<Sprite>("BloonTypeStaticOn");
        private static Sprite StaticOffIcon => Resources.GetResource<Sprite>("BloonTypeStaticOff");
        private static Sprite TatteredOnIcon => Resources.GetResource<Sprite>("BloonTypeTatteredOn");
        private static Sprite TatteredOffIcon => Resources.GetResource<Sprite>("BloonTypeTatteredOff");
        private static Sprite LeadOnIcon => Resources.GetResource<Sprite>("BloonTypeLeadOn");
        private static Sprite LeadOffIcon => Resources.GetResource<Sprite>("BloonTypeLeadOff");

        private static bool shielded, @static, tattered, lead;
        public static bool Shielded { get => shielded; set => shielded = value; }
        public static bool Static { get => @static; set => @static = value; }
        public static bool Tattered { get => tattered; set => tattered = value; }
        public static bool Lead { get => lead; set => lead = value; }
        public static bool Any => Shielded || Static || Tattered || Lead;

        void Start() {
            PopoutBtn.onClick.AddListener(new System.Action(() => ToggleOpen()));

            Button[] buttons = GetComponentsInChildren<Button>();
            Image[] images = GetComponentsInChildren<Image>();

            Button shieldedBtn = buttons[0], staticBtn = buttons[1], tatteredBtn = buttons[2], leadBtn = buttons[3];
            Image shieldedIco = images[1], staticIco = images[2], tatteredIco = images[3], leadIco = images[4];

            shieldedBtn.onClick.AddListener(new System.Action(() => ToggleButton(shieldedIco, ShieldedOnIcon, ShieldedOffIcon, staticIco, StaticOffIcon, ref shielded, ref @static)));

            staticBtn.onClick.AddListener(new System.Action(() => ToggleButton(staticIco, StaticOnIcon, StaticOffIcon, shieldedIco, ShieldedOffIcon, ref @static, ref shielded)));

            tatteredBtn.onClick.AddListener(new System.Action(() => ToggleButton(tatteredIco, TatteredOnIcon, TatteredOffIcon, leadIco, LeadOffIcon, ref tattered, ref lead)));

            leadBtn.onClick.AddListener(new System.Action(() => ToggleButton(leadIco, LeadOnIcon, LeadOffIcon, tatteredIco, TatteredOffIcon, ref lead, ref tattered)));
        }

        public void Init(Transform popoutBtn, BloonMenu bloonMenu) {
            PopoutBtn = popoutBtn.GetComponent<Button>();
            PopoutImg = popoutBtn.GetComponent<Image>();
            BloonMenu = bloonMenu;
            WasMenuShowing = BloonMenu.showing;

            Shielded = false;
            Static = false;
            Tattered = false;
            Lead = false;

            closedX = transform.localPosition.x;
            openX = closedX - openMovement;
            popoutBtnOffset = popoutBtn.localPosition.x - closedX;
        }

        public void Update() {
            if (!BloonMenu.showing && WasMenuShowing) {
                WasOpen = Open;
                if (Open)
                    ToggleOpen(false);
            } else if (BloonMenu.showing && !WasMenuShowing) {
                if (WasOpen)
                    ToggleOpen(true);
            }
            WasMenuShowing = BloonMenu.showing;

            float newX = Open ? Mathf.SmoothStep(closedX, openX, t) : Mathf.SmoothStep(openX, closedX, t);
            transform.localPosition = new(newX, transform.localPosition.y);
            PopoutBtn.transform.localPosition = new(newX + popoutBtnOffset, PopoutBtn.transform.localPosition.y);

            t += 10f * Time.deltaTime;
            t = Mathf.Clamp(t, 0, 1);
        }

        private void ToggleButton(Image icon, Sprite onIcon, Sprite offIcon, Image opIcon, Sprite opOffIcon, ref bool toggle, ref bool opToggle) {
            toggle = !toggle;
            icon.sprite = toggle ? onIcon : offIcon;
            icon.transform.Cast<RectTransform>().sizeDelta = toggle ? onSize : offSize;
            if (toggle) {
                opToggle = false;
                opIcon.sprite = opOffIcon;
                opIcon.transform.Cast<RectTransform>().sizeDelta = offSize;
            }

            BloonMenu.SortBloons();
        }

        private void ToggleOpen(bool? isOpen = null) {
            Open = isOpen ?? !Open;
            t = 1 - t;
            PopoutImg.sprite = Resources.GetResource<Sprite>(Open ? "ArrowRight" : "ArrowLeft");
        }
#endif
    }
}