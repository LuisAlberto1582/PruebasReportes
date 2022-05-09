<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BusquedaExternaCCustodia.aspx.cs"
    Inherits="AppExtNextel.BusquedaExternaCCustodia.BusquedaExternaCCustodia" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Busqueda de Cartas Custodia</title>
    <link href="../Styles/keytia.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div>
            &nbsp;&nbsp;&nbsp;
            <br />
            &nbsp;&nbsp;
            <img src="../images/nextel.jpg" />&nbsp;&nbsp;
            <img src="../images/KeytiaHeader.png" />
            <br />
            <br />
            <br />
            <br />
        </div>
        <div class="headerCCustodia" style="height: 15px;">
            <div align="center">
                <br />
                <br />
                <br />
                <br />
                <%--<asp:Label ID="lblBusquedaCCustodia" runat="server" Text="Búsqueda de Cartas Custodia"
                Font-Bold="true">
            </asp:Label>--%>
                <label id="lblBusquedaCCustodia" runat="server" style="font-weight: bold; font-size: 18px;">
                    Búsqueda de Cartas Custodia</label>
            </div>
            <div>
                <br />
                <br />
                <asp:Table ID="tblBusquedaCCustodia" runat="server" CaptionAlign="Top" HorizontalAlign="Center">
                    <asp:TableRow>
                        <asp:TableCell>
                            <%--  <asp:Label ID="lblFolio" runat="server" Text="Folio:">   
                        </asp:Label>--%>
                            <label id="lblFolio" runat="server">
                                Folio:</label>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="txtFolio" runat="server">
                            </asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell>
                            <%--<asp:Label ID="lblExtension" runat="server" Text="Extensión:">   
                        </asp:Label>--%>
                            <label id="lblExtension" runat="server">
                                Extensión:</label>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="txtExtension" runat="server">
                            </asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell>
                            <%--<asp:Label ID="lblNombre" runat="server" Text="Nombre(s):">   
                        </asp:Label>--%>
                            <label id="lblNombre" runat="server">
                                Nombre(s):</label>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="txtNombre" runat="server">
                            </asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell>
                            <%--<asp:Label ID="lblApellidos" runat="server" Text="Apellidos (paterno y/o materno):">   
                        </asp:Label>--%>
                            <label id="lblApellidos" runat="server">
                                Apellidos (paterno y/o materno):</label>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox ID="txtApellidos" runat="server">
                            </asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
            </div>
            <div align="center">
                <br />
                <br />
                <asp:Button ID="btnBuscarCCustodia" runat="server" Text="Buscar" CssClass="botonesBusqExtCCust"
                    OnClick="btnBuscarCCustodia_Click" />
            </div>
            <div>
                <asp:Table ID="tblResultados" runat="server" CaptionAlign="Left" HorizontalAlign="Center">
                    <asp:TableRow>
                        <asp:TableCell>
                            <%--<asp:Label ID="lblCartasEncontradas" runat="server" Text="Cartas encontradas: " Font-Bold="true">
                        </asp:Label>--%>
                            <label id="lblCartasEncontradas" runat="server" style="font-weight: bold; font-size: 18px;">
                                Cartas encontradas:
                            </label>
                        </asp:TableCell>
                        <asp:TableCell>
                            <asp:Label ID="lblCartasEncontradasCount" runat="server" Text="">
                            </asp:Label>
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
            </div>
            <br />
            <br />
            <div>
                <asp:GridView ID="grvCCustodia" runat="server" RowStyle-HorizontalAlign="Center" AutoGenerateColumns="false"
                    ShowFooter="true" CssClass="GridView"
                    HorizontalAlign="Center">
                    <Columns>
                        <asp:BoundField DataField="FolioCCustodia" HeaderText="Folio" HtmlEncode="true" />
                        <asp:HyperLinkField HeaderText="Nombre" DataNavigateUrlFields="EmpleCatalogo" DataNavigateUrlFormatString="~/CCustodia/AppCCustodia.aspx?Opc=OpcAppCCustodia&iCodEmple={0}&st=jq9g9FcOjmI="
                            DataTextField="NomCompleto" />
                        <%--                <asp:BoundField DataField="NomCompleto" HeaderText="Nombre" HtmlEncode = "true" />--%>
                        <asp:BoundField DataField="Email" HeaderText="Email" HtmlEncode="true" />
                    </Columns>
                    <HeaderStyle BackColor="#2E6E9E" ForeColor="White" />
                    <RowStyle CssClass="GridRowOdd" />
                    <AlternatingRowStyle CssClass="GridRowEven" />
                </asp:GridView>
            </div>
            <br />
            <br />
            <div align="center">
                <asp:Button ID="btnRegresar" runat="server" Text="Regresar" Visible="false" OnClick="btnRegresar_Click"
                    CssClass="botonesBusqExtCCust" Height="26px" />
            </div>
        </div>
    </form>
</body>
</html>
