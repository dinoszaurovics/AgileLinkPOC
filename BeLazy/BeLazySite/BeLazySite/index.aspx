<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="BeLazySite.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:BeLazy_DBConnectionString %>" SelectCommand="SELECT [LogMessageID], [DateOfMessage], [ErrorLevel], [ErrorMessage] FROM [tLogMessages] ORDER BY [DateOfMessage] DESC"></asp:SqlDataSource>
            <asp:GridView ID="GridView1" runat="server" AllowSorting="True" AutoGenerateColumns="False" DataKeyNames="LogMessageID" DataSourceID="SqlDataSource1">
                <Columns>
                    <asp:BoundField DataField="LogMessageID" HeaderText="LogMessageID" InsertVisible="False" ReadOnly="True" SortExpression="LogMessageID" />
                    <asp:BoundField DataField="DateOfMessage" HeaderText="DateOfMessage" SortExpression="DateOfMessage" />
                    <asp:BoundField DataField="ErrorLevel" HeaderText="ErrorLevel" SortExpression="ErrorLevel" />
                    <asp:BoundField DataField="ErrorMessage" HeaderText="ErrorMessage" SortExpression="ErrorMessage" />
                </Columns>
            </asp:GridView>

        </div>
    </form>
</body>
</html>
