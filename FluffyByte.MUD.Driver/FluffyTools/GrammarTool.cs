/*
 * (GrammarTool.cs)
 *------------------------------------------------------------
 * Created - 11/25/2025 11:50:40 PM
 * Created by - Seliris
 *-------------------------------------------------------------
 */

using System.Text;

namespace FluffyByte.MUD.Driver.FluffyTools;

public static class GrammarTool
{
    public static string ToCommaList(this IEnumerable<string> items)
    {
        List<string> list = [.. items];

        if (list.Count == 0)
            return string.Empty;

        if (list.Count == 1)
            return list[0];

        if (list.Count == 2)
            return $"{list[0]} and {list[1]}";

        StringBuilder sb = new();

        for(int i = 0; i < list.Count; i++)
        {
            if(i == list.Count - 1)
            {
                sb.Append($"and {list[i]}");
            }
            else
            {
                sb.Append($"{list[i]}, ");
            }
        }

        sb.Append('.');

        return sb.ToString();
    }
}

/*
*------------------------------------------------------------
* (GrammarTool.cs)
* See License.txt for licensing information.
*-----------------------------------------------------------
*/