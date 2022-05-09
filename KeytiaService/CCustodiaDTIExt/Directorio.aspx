<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Directorio.aspx.cs" Inherits="CCustodiaDTIExt.Directorio" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="Styles/keytia.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="formDirectorio" runat="server">
    <div>
        &nbsp;&nbsp;&nbsp;
        <br />
        &nbsp;&nbsp;
        <img src="images/nextel.jpg" />&nbsp;&nbsp;
        <img src="images/KeytiaHeader.png" />
        <br />
        <br />
        <br />
        <br />
    </div>
    <div class="headerCCustodia" style="height: 15px;">
    </div>
    <br />
    <br />
    <asp:PlaceHolder ID="divBusqueda" runat="server" Visible="true">
        <asp:Label ID="lblTitulo" runat="server" Text="En esta sección se podrá hacer una búsqueda de cualquier empleado"
            CssClass="LabelsDirectorio"></asp:Label>
        <asp:Table ID="Table1" runat="server" CellPadding="5" Width="80%" HorizontalAlign="Center">
            <asp:TableHeaderRow>
                <asp:TableCell ColumnSpan="2" HorizontalAlign="Center">
                    <asp:Label ID="lblInstrucciones" runat="server" Text="DIRECTORIO" CssClass="LabelsDirectorio"></asp:Label>
                </asp:TableCell>
            </asp:TableHeaderRow>
            <asp:TableRow Width="100%">
                <asp:TableCell HorizontalAlign="Right" Width="25%">
                    <asp:Label ID="lblNomina" runat="server" Text="Nómina" CssClass="LabelsDirectorio"></asp:Label>
                </asp:TableCell>
                <asp:TableCell HorizontalAlign="Left" Width="75%">
                    <asp:TextBox ID="txtNomina" runat="server" Width="75%"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell HorizontalAlign="Right">
                    <asp:Label ID="lblNombre" runat="server" Text="Nombre(s)" CssClass="LabelsDirectorio"></asp:Label>
                </asp:TableCell>
                <asp:TableCell HorizontalAlign="Left">
                    <asp:TextBox ID="txtNombre" runat="server" Width="75%"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell HorizontalAlign="Right">
                    <asp:Label ID="lblApellidos" runat="server" Text="Apellidos (paterno y/o materno)"
                        CssClass="LabelsDirectorio"></asp:Label>
                </asp:TableCell>
                <asp:TableCell HorizontalAlign="Left">
                    <asp:TextBox ID="txtApellidos" runat="server" Width="75%"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell HorizontalAlign="Right">
                    <asp:Label ID="lblExtension" runat="server" Text="Extensión" CssClass="LabelsDirectorio"></asp:Label>
                </asp:TableCell>
                <asp:TableCell HorizontalAlign="Left">
                    <asp:TextBox ID="txtExtension" runat="server" Width="75%"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell HorizontalAlign="Right">
                    <asp:Label ID="lblEmpresa" runat="server" Text="Empresa" CssClass="LabelsDirectorio"></asp:Label>
                </asp:TableCell>
                <asp:TableCell HorizontalAlign="Left">
                    <asp:TextBox ID="txtEmpresa" align="Center" runat="server" Width="75%"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell HorizontalAlign="Center" ColumnSpan="2">
                    <asp:Button ID="btnBuscar" runat="server" Text="Buscar" UseSubmitBehavior="true"
                        CssClass="botonesBusqExtCCust" />
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:PlaceHolder>
    <div style="text-align: center">
        <asp:Panel ID="divResultados" runat="server" Visible="false">
            <asp:Label ID="lblInformacion" runat="server" CssClass="LabelsDirectorio">Seleccionar el registro apropiado de la siguiente lista</asp:Label>
            <br />
            <asp:Label ID="Label1" runat="server"></asp:Label>
            <br />
            <asp:GridView ID="gvAgrupado" runat="server" RowStyle-HorizontalAlign="Center" Width="80%"
                CssClass="GridView" AutoGenerateColumns="false" ShowFooter="true" 
                HorizontalAlign="Center">
                <Columns>
                    <asp:BoundField DataField="Nomina" HeaderText="Nómina" HtmlEncode="true" />
                    <asp:HyperLinkField DataNavigateUrlFields="iCodCatalogo" DataNavigateUrlFormatString="Resultados.aspx?Empleado={0}"
                        DataTextField="Nombre" HeaderText="Nombre" NavigateUrl="Resultados.aspx" />
                    <asp:BoundField DataField="Email" HeaderText="Email" HtmlEncode="true" />
                    <asp:BoundField DataField="Empresa" HeaderText="Empresa" HtmlEncode="true" />
                </Columns>               
            </asp:GridView>
            <br />
            <asp:Button ID="btnBuscarOtro" runat="server" Text="Buscar Otro" OnClick="btnBuscarOtro_Click"
                CssClass="botonesBusqExtCCust" />
        </asp:Panel>
    </div>
    </form>
</body>
</html>
