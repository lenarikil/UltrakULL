using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UltrakULL
{
    /// <summary>
    /// Live language switching for texts that type themselves out.
    ///
    /// The game types such texts from a string the mod translated when the typing
    /// started, so a language switch has to reach the typing itself: the string is
    /// swapped for the new language and the typing continues from the
    /// proportionally same position. The share of the text that is already on
    /// screen stays the same, so the new language is not revealed ahead of the
    /// progress the player has seen, and nothing starts over.
    /// </summary>
    public static class LiveTypingText
    {
        /// <summary>
        /// Maps the typing progress of one text onto another one of a different
        /// length: the same share of the full text stays visible.
        /// </summary>
        public static int MapVisibleCount(int oldVisible, int oldTotal, int newTotal)
        {
            if (oldTotal <= 0 || newTotal <= 0 || oldVisible <= 0)
                return 0;

            if (oldVisible >= oldTotal)
                return newTotal;

            long mapped = (long)Math.Floor((double)oldVisible * newTotal / oldTotal);
            if (mapped > newTotal)
                mapped = newTotal;

            return (int)mapped;
        }

        /// <summary>
        /// The number of characters a string shows on screen. Rich text tags are
        /// formatting and not content, so they are not part of the progress.
        /// </summary>
        public static int CountVisibleChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int count = 0;
            bool insideTag = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (insideTag)
                {
                    if (c == '>')
                        insideTag = false;
                }
                else if (c == '<')
                {
                    insideTag = true;
                }
                else
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// The position in a string at which the given number of visible characters
        /// has been typed. The position never rests inside a rich text tag: the
        /// typing of the game skips a tag in one step, so a tag that starts at the
        /// position is left for that same skip.
        /// </summary>
        public static int IndexForVisibleCount(string text, int visibleCount)
        {
            if (string.IsNullOrEmpty(text) || visibleCount <= 0)
                return 0;

            int count = 0;
            bool insideTag = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (insideTag)
                {
                    if (c == '>')
                        insideTag = false;
                    continue;
                }

                if (c == '<')
                {
                    insideTag = true;
                    continue;
                }

                if (count >= visibleCount)
                    return i;

                count++;
            }

            return text.Length;
        }

        /// <summary>
        /// True while the component is typing its message out.
        /// </summary>
        public static bool IsTyping(ScrollingText scrolling)
        {
            if (scrolling == null)
                return false;

            try
            {
                Coroutine routine = ReflectionUtils.GetPrivate<ScrollingText, Coroutine>(
                    scrolling, typeof(ScrollingText), "messageRoutine");
                return routine != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Swaps the message a ScrollingText is typing for another language.
        ///
        /// The running coroutine keeps its own copy of the message and of its
        /// letter index, so it is stopped and replaced by a continuation that
        /// starts at the proportionally same position. The message field is updated
        /// as well, so the component stays consistent for everything the game does
        /// with it later (finishing the text when it is disabled, for example).
        /// </summary>
        public static void SwapScrollingMessage(ScrollingText scrolling, string newMessage)
        {
            if (scrolling == null || scrolling.text == null || string.IsNullOrEmpty(newMessage))
                return;

            try
            {
                TMP_Text text = scrolling.text;
                string oldMessage = scrolling.message;

                // The typing writes the message as a growing prefix plus the cursor
                // markup, so the prefix the text shares with the message is exactly
                // what has been typed so far.
                int typedLetter = CommonPrefixLength(text.text, oldMessage);
                if (typedLetter > oldMessage.Length)
                    typedLetter = oldMessage.Length;

                int oldVisible = CountVisibleChars(oldMessage.Substring(0, typedLetter));
                int oldTotal = CountVisibleChars(oldMessage);
                int newTotal = CountVisibleChars(newMessage);
                int newVisible = MapVisibleCount(oldVisible, oldTotal, newTotal);
                int newLetter = IndexForVisibleCount(newMessage, newVisible);

                Coroutine routine = ReflectionUtils.GetPrivate<ScrollingText, Coroutine>(
                    scrolling, typeof(ScrollingText), "messageRoutine");
                if (routine != null)
                    scrolling.StopCoroutine(routine);

                scrolling.message = newMessage;
                WriteTypingState(text, newMessage, newLetter, scrolling.fillMissingText, scrolling.writingCursor);

                // The continuation runs on the component itself, so it dies with it,
                // and the routine field is kept in sync for the game's OnDisable.
                Coroutine continuation = scrolling.StartCoroutine(ContinueShowText(
                    scrolling, text, newMessage, newLetter, scrolling.secondsBetweenLetters, scrolling.aud,
                    scrolling.fillMissingText, false, scrolling.writingCursor));
                ReflectionUtils.SetPrivate(scrolling, typeof(ScrollingText), "messageRoutine", continuation);
            }
            catch (Exception e)
            {
                Logging.Warn("[LiveTypingText] Failed to swap a typing text: " + e.Message);
            }
        }

        /// <summary>
        /// Writes the text exactly the way the game's typing does at a letter
        /// position: the typed prefix, plus the cursor markup the component uses.
        /// </summary>
        private static void WriteTypingState(TMP_Text text, string message, int currentLetter, bool fillMissingText, bool writingCursor)
        {
            text.text = message.Substring(0, currentLetter);
            if (currentLetter >= message.Length)
                return;

            if (fillMissingText)
            {
                if (writingCursor)
                {
                    text.text = text.text + "<mark=#" + ColorUtility.ToHtmlStringRGB(text.color) + ">"
                        + message[currentLetter] + "</mark><alpha=#00>" + message.Substring(currentLetter + 1);
                }
                else
                {
                    text.text = text.text + "<alpha=#00>" + message.Substring(currentLetter);
                }
            }
            else if (writingCursor)
            {
                text.text += "█";
            }
        }

        /// <summary>
        /// The typing of ScrollingText.ShowText, continued from a given letter. The
        /// body is the game's own logic: tags are skipped in one step, the cursor
        /// markup follows the component's settings, and the click sound plays per
        /// typed character. The epilogue is the game's PrepText: the completion
        /// event fires and the routine field is cleared.
        /// </summary>
        private static IEnumerator ContinueShowText(
            ScrollingText scrolling,
            TMP_Text text,
            string message,
            int startLetter,
            float secondsBetweenLetters,
            AudioSource clickAudio,
            bool fillMissingText,
            bool skipLineBreaks,
            bool writingCursor)
        {
            TimeSince textTimer = 0f;
            int currentLetter = startLetter;
            while (currentLetter < message.Length)
            {
                while ((float)textTimer >= secondsBetweenLetters && currentLetter < message.Length)
                {
                    textTimer = (float)textTimer - secondsBetweenLetters;
                    if (message[currentLetter] == '<')
                    {
                        for (; message[currentLetter] != '>' && currentLetter <= message.Length; currentLetter++)
                        {
                        }
                    }
                    else if (currentLetter < message.Length - 1)
                    {
                        for (; currentLetter < message.Length - 1 && ((!writingCursor && message[currentLetter + 1] == ' ') || (skipLineBreaks && message[currentLetter + 1] == '\n')); currentLetter++)
                        {
                        }
                    }
                    currentLetter++;
                    WriteTypingState(text, message, currentLetter, fillMissingText, writingCursor);
                    if ((bool)clickAudio && message[currentLetter - 1] != '\n' && message[currentLetter - 1] != ' ')
                    {
                        clickAudio.Play(tracked: true);
                    }
                }
                yield return new WaitForSeconds(secondsBetweenLetters);
            }

            if (scrolling != null && scrolling.onComplete != null)
                scrolling.onComplete.Invoke();

            if (scrolling != null)
                ReflectionUtils.SetPrivate<ScrollingText, Coroutine>(scrolling, typeof(ScrollingText), "messageRoutine", null);
        }

        private static int CommonPrefixLength(string a, string b)
        {
            if (a == null || b == null)
                return 0;

            int limit = Math.Min(a.Length, b.Length);
            int i = 0;
            while (i < limit && a[i] == b[i])
                i++;

            return i;
        }
    }
}
