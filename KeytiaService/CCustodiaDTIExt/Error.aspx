<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="CCustodiaDTIExt.Error" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Error</title>
    <link href="Styles/keytia.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div align="center">
            <br />
            <asp:Image runat="server" ImageUrl="~/images/KeytiaHeader.png" />
            <br />
            <br />
        </div>
        <div class="headerCCustodia" style="height: 15px;">
            <div align="center">
                <br />
                <br />
                <br />
                <br />
                <label id="lblBusquedaCCustodia" runat="server" style="font-weight: bold; font-size: 12px;">
                    Ocurrió un error en la página.</label>
            </div>
        </div>
    </div>
    </form>
</body>
</html>
