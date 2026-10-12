using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DeadManDraws
{
    public class UiAnimation : MonoBehaviour
    {
        [Header("References")]
        public GameUIController Controller;

        [Header("Timing")]
        public float DrawDuration = 0.35f;
        public float BurnDuration = 0.7f;
        public float CollectDuration = 0.5f;

        [Header("Scales")]
        public float StartScale = 0.4f;
        public float PeakScale = 1.15f;

        [Header("Debug")]
        public bool VerboseLog = true;

        private RectTransform _animLayer;
        private Canvas _canvas;

        // ============================================================
        // Layer
        // ============================================================
        private void EnsureLayer()
        {
            if (_animLayer != null) return;
            if (Controller == null) return;

            _canvas = Controller.GetComponentInParent<Canvas>();
            if (_canvas == null)
            {
                if (VerboseLog) Debug.LogWarning("[UiAnimation] No Canvas found in parents of Controller.");
                return;
            }

            var go = new GameObject("AnimLayer", typeof(RectTransform));
            _animLayer = go.GetComponent<RectTransform>();
            _animLayer.SetParent(_canvas.transform, false);
            _animLayer.anchorMin = Vector2.zero;
            _animLayer.anchorMax = Vector2.one;
            _animLayer.offsetMin = Vector2.zero;
            _animLayer.offsetMax = Vector2.zero;
            _animLayer.SetAsLastSibling();
        }

        private Image SpawnTemp(Sprite sprite, Vector3 worldPos, float width, float scale)
        {
            EnsureLayer();
            if (_animLayer == null) { if (VerboseLog) Debug.LogWarning("[UiAnimation] AnimLayer null, cannot spawn."); return null; }
            if (sprite == null) { if (VerboseLog) Debug.LogWarning("[UiAnimation] Sprite null."); return null; }

            var go = new GameObject("TmpCard", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(_animLayer, false);
            rt.sizeDelta = new Vector2(width, width * 1.4f);
            rt.position = worldPos;
            rt.localScale = Vector3.one * scale;
            rt.SetAsLastSibling();

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            img.preserveAspect = true;
            return img;
        }

        // ============================================================
        // DRAW
        // ============================================================
        public void AnimateDrawCard(CardDefinition c)
        {
            if (c == null || c.Image == null || Controller == null) return;
            if (Controller.DrawDeckImage == null) return;

            var slot = Controller.GetNextFreeSlot();
            if (slot == null) return;

            StartCoroutine(DrawRoutine(c.Image, Controller.DrawDeckImage.rectTransform, slot));
        }

        private IEnumerator DrawRoutine(Sprite sprite, RectTransform from, RectTransform to)
        {
            float w = from.rect.width > 1f ? from.rect.width : 100f;
            var img = SpawnTemp(sprite, from.position, w, StartScale);
            if (img == null) yield break;

            var rt = img.rectTransform;
            float t = 0f;
            while (t < DrawDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / DrawDuration);
                float eased = Mathf.SmoothStep(0f, 1f, k);

                rt.position = Vector3.Lerp(from.position, to.position, eased);

                float scale = (k < 0.5f)
                    ? Mathf.Lerp(StartScale, PeakScale, k * 2f)
                    : Mathf.Lerp(PeakScale, 1f, (k - 0.5f) * 2f);
                rt.localScale = Vector3.one * scale;

                float yaw = Mathf.Lerp(180f, 0f, eased);
                rt.localRotation = Quaternion.Euler(0f, yaw, 0f);

                yield return null;
            }

            Destroy(rt.gameObject);
        }

        // ============================================================
        // BURN
        // ============================================================
        public void AnimateBurnCards(PlayerData p)
        {
            if (VerboseLog)
                Debug.Log($"[Burn] called. player={(p != null ? p.PlayerName : "NULL")}, " +
                          $"board={(p != null ? p.Board.Count : 0)}, " +
                          $"controller={(Controller != null)}, " +
                          $"burnDeck={(Controller != null && Controller.BurnDeckImage != null)}, " +
                          $"slots={(Controller != null && Controller.Slots != null ? Controller.Slots.Count : 0)}");

            if (p == null || Controller == null) return;
            if (Controller.BurnDeckImage == null) return;
            if (Controller.Slots == null || Controller.Slots.Count == 0) return;

            var target = Controller.BurnDeckImage.rectTransform;
            int n = Mathf.Min(p.Board.Count, Controller.Slots.Count);

            for (int i = 0; i < n; i++)
            {
                var card = p.Board[i];
                var slot = Controller.Slots[i];

                if (VerboseLog)
                    Debug.Log($"[Burn] i={i}, card={(card != null ? card.Name : "NULL")}, " +
                              $"cardImg={(card != null && card.Image != null)}, slot={(slot != null)}");

                if (card == null || card.Image == null) continue;
                if (slot == null) continue;

                StartCoroutine(BurnRoutine(slot, target, card.Image));
            }
        }

        private IEnumerator BurnRoutine(Image slot, RectTransform target, Sprite sprite)
        {
            Vector3 startPos = slot.rectTransform.position;
            Vector3 endPos = target.position;

            float w = slot.rectTransform.rect.width > 1f ? slot.rectTransform.rect.width : 100f;
            var img = SpawnTemp(sprite, startPos, w, 1f);
            if (img == null) yield break;

            slot.enabled = false;

            var rt = img.rectTransform;
            var cg = img.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            float t = 0f;
            while (t < BurnDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / BurnDuration);
                float eased = Mathf.SmoothStep(0f, 1f, k);

                rt.position = Vector3.Lerp(startPos, endPos, eased);
                rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.15f, eased);
                rt.localRotation = Quaternion.Euler(0f, Mathf.Lerp(0f, 540f, eased), 0f);
                cg.alpha = Mathf.Lerp(1f, 0f, eased);

                yield return null;
            }

            Destroy(rt.gameObject);
        }

        // ============================================================
        // COLLECT
        // ============================================================
        public void AnimateCollect(PlayerData p, System.Action onDone)
        {
            StartCoroutine(CollectRoutine(p, onDone));
        }

        private IEnumerator CollectRoutine(PlayerData p, System.Action onDone)
        {
            if (p == null || Controller == null || Controller.Slots == null || p.Board.Count == 0)
            {
                yield return new WaitForSeconds(0.2f);
                onDone?.Invoke();
                yield break;
            }

            int n = Mathf.Min(p.Board.Count, Controller.Slots.Count);
            float stagger = 0.06f;

            for (int i = 0; i < n; i++)
            {
                var slot = Controller.Slots[i];
                if (slot == null) continue;

                var card = p.Board[i];
                if (card == null || card.Image == null) continue;

                Vector3 from = slot.rectTransform.position;
                Vector3 to;

                if (p.IsHuman)
                {
                    var bankSlot = Controller.GetBankSlotForType(card.CardType);
                    to = bankSlot != null ? bankSlot.position : Controller.GetBankTargetPosition();
                }
                else
                {
                    var panel = Controller.GetPlayerPanelRT(p.PlayerId);
                    to = panel != null ? panel.position : Controller.GetBankTargetPosition();
                }

                StartCoroutine(FlyTo(from, to, card.Image, slot));
                yield return new WaitForSeconds(stagger);
            }

            yield return new WaitForSeconds(CollectDuration);
            onDone?.Invoke();
        }

        private IEnumerator FlyTo(Vector3 from, Vector3 to, Sprite sprite, Image slot)
        {
            float w = slot.rectTransform.rect.width > 1f ? slot.rectTransform.rect.width : 100f;
            var img = SpawnTemp(sprite, from, w, 1f);
            if (img == null) yield break;

            slot.enabled = false;

            var rt = img.rectTransform;
            float t = 0f;
            while (t < CollectDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / CollectDuration);
                float eased = Mathf.SmoothStep(0f, 1f, k);
                rt.position = Vector3.Lerp(from, to, eased);
                rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.4f, eased);
                yield return null;
            }
            Destroy(rt.gameObject);
        }

        // ============================================================
        public void PulseElement(RectTransform rt, float duration = 0.3f)
        {
            if (rt == null) return;
            StartCoroutine(PulseRoutine(rt, duration));
        }

        private IEnumerator PulseRoutine(RectTransform rt, float duration)
        {
            var baseScale = rt.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = t / duration;
                float s = 1f + Mathf.Sin(k * Mathf.PI) * 0.25f;
                rt.localScale = baseScale * s;
                yield return null;
            }
            rt.localScale = baseScale;
        }
    }
}