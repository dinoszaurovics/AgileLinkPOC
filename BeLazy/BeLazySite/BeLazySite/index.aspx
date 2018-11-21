<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="BeLazySite.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="Onboarding check" />
            &nbsp;&nbsp;
            <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="Clear log" />
            &nbsp;&nbsp;
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Project syncronization" />
        </div>
        <br />
        <div>
            
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:BeLazy_DBConnectionString %>"
                SelectCommand="SELECT [LogMessageID], [DateOfMessage], [ErrorLevel], [ErrorMessage] FROM [tLogMessages] ORDER BY [DateOfMessage] DESC"></asp:SqlDataSource>
            <asp:GridView ID="GridView1" runat="server" AllowSorting="True" AutoGenerateColumns="False" DataKeyNames="LogMessageID" DataSourceID="SqlDataSource1">
                <Columns>
                    <asp:BoundField DataField="LogMessageID" HeaderText="LogMessageID" InsertVisible="False" ReadOnly="True" SortExpression="LogMessageID" />
                    <asp:BoundField DataField="DateOfMessage" HeaderText="DateOfMessage" SortExpression="DateOfMessage" />
                    <asp:BoundField DataField="ErrorLevel" HeaderText="ErrorLevel" SortExpression="ErrorLevel" />
                    <asp:BoundField DataField="ErrorMessage" HeaderText="ErrorMessage" SortExpression="ErrorMessage" />
                </Columns>
            </asp:GridView>

        </div>
        <br />
        <div>
            
            <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:BeLazy_DBConnectionString %>"
                SelectCommand="SELECT DateOrdered, ExternalProjectCode, EndCustomer, ExternalProjectManagerName, Deadline, PayableVolume, UnitName FROM tProject p 
INNER JOIN tUnits u ON p.PayableUnitID = u.UnitID ORDER BY DateOrdered DESC"></asp:SqlDataSource>
            <asp:GridView ID="GridView2" runat="server" AllowSorting="True" AutoGenerateColumns="False" DataKeyNames="ExternalProjectCode" DataSourceID="SqlDataSource2">
                <Columns>
                    <asp:BoundField DataField="DateOrdered" HeaderText="DateOrdered" SortExpression="DateOrdered" />
                    <asp:BoundField DataField="ExternalProjectCode" HeaderText="ExternalProjectCode" InsertVisible="False" ReadOnly="True" SortExpression="ExternalProjectCode" />
                    <asp:BoundField DataField="EndCustomer" HeaderText="EndCustomer" SortExpression="EndCustomer" />
                    <asp:BoundField DataField="ExternalProjectManagerName" HeaderText="ExternalProjectManagerName" SortExpression="ExternalProjectManagerName" />
                    <asp:BoundField DataField="Deadline" HeaderText="Deadline" SortExpression="Deadline" />
                    <asp:BoundField DataField="PayableVolume" HeaderText="PayableVolume" SortExpression="PayableVolume" />
                    <asp:BoundField DataField="UnitName" HeaderText="UnitName" SortExpression="UnitName" />
                </Columns>
            </asp:GridView>

        </div>
    </form>
</body>
</html>
