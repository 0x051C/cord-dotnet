using Cord.Core;

namespace Cord.Tests;

public class DelimitedFileTests
{
    [Fact]
    public void Skips_the_header_and_blank_lines_and_keeps_quotes_verbatim()
    {
        var text = "a^b^c\r\n1^LLC \"X\"^3\r\n\r\n ^ ^ \n4^5^6\n";

        var rows = DelimitedFile.ReadRows(new StringReader(text)).ToList();

        Assert.Equal([["1", "LLC \"X\"", "3"], ["4", "5", "6"]], rows);
    }

    [Fact]
    public void An_empty_file_has_no_rows()
    {
        Assert.Empty(DelimitedFile.ReadRows(new StringReader("")));
        Assert.Empty(DelimitedFile.ReadRows(new StringReader("header^only\n")));
    }
}
