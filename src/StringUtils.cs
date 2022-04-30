using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule
{
    public static class StringUtils
    {
        public static string SanitizeHtml(string input)
        {
            input = RemoveStyleTag(input);
            input = RemoveScriptTag(input);
            var currentIndex = 0;
            while (currentIndex != -1)
            {
                currentIndex = input.IndexOf('<', currentIndex);

                if (currentIndex == -1)
                {
                    break;
                }

                var endIndex = input.IndexOf('>', currentIndex);

                input = input.Remove(currentIndex, endIndex - currentIndex + 1);

                currentIndex = 0;
            }

            return input;
        }

        private static string RemoveStyleTag(string input)
        {
            var currentIndex = 0;
            while (currentIndex != -1)
            {
                currentIndex = input.IndexOf("<style", currentIndex);

                if (currentIndex == -1)
                {
                    break;
                }

                var endIndex = input.IndexOf("</style>", currentIndex);

                input = input.Remove(currentIndex, endIndex - currentIndex + "</style>".Length);

                currentIndex = 0;
            }

            return input;
        }

        private static string RemoveScriptTag(string input)
        {
            var currentIndex = 0;
            while (currentIndex != -1)
            {
                currentIndex = input.IndexOf("<script", currentIndex);

                if (currentIndex == -1)
                {
                    break;
                }

                var endIndex = input.IndexOf("</script>", currentIndex);

                input = input.Remove(currentIndex, endIndex - currentIndex + "</script>".Length);

                currentIndex = 0;
            }

            return input;
        }
    }
}
