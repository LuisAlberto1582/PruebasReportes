<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Resultados.aspx.cs" Inherits="AppExtNextel.Resultados" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="Styles/keytia.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
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
        <br />
        <br />
        <div>
            <div style="text-align: center">
                <asp:PlaceHolder ID="divDetallado" runat="server" Visible="true">
                    <asp:Table ID="DETALLE" runat="server" CellPadding="5" Width="70%" HorizontalAlign="Center">
                        <asp:TableHeaderRow>
                            <asp:TableCell ColumnSpan="4" HorizontalAlign="Center">
                                <label id="Label2" runat="server" style="font-weight: bold; color: black; font-size: medium">
                                    DATOS DEL EMPLEADO</label>        
                            </asp:TableCell>                          
                        </asp:TableHeaderRow>
                        <asp:TableRow Width="100%">
                            <asp:TableCell HorizontalAlign="Right" Width="20%">
                                <label id="detallNombre" runat="server">
                                    Nombre(s):</label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Left" Width="30%">
                                <label style="font-weight: normal"  id="txtDetallNombre" align="Center" runat="server" width="35%">
                                </label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Right" Width="20%">
                                <label id="detallTipoEmple" runat="server">
                                    Tipo de Empleado:</label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Left" Width="30%">
                                <label style="font-weight: normal"  id="txtDetallTipoEmple" align="Center" runat="server" width="35%">
                                </label>
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="Right" Width="20%">
                                <label id="detallPaterno" runat="server">
                                    Apellido Paterno:</label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Left" Width="30%">
                                <label style="font-weight: normal"  id="txtDetallPaterno" align="Center" runat="server" width="35%">
                                </label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Right" Width="20%">
                                <label id="detallSitio" runat="server">
                                    Sitio:</label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Left" Width="30%">
                                <label style="font-weight: normal"  id="txtDetallSitio" align="Center" runat="server" width="35%">
                                </label>
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="Right" Width="20%">
                                <label id="detallMaterno" runat="server">
                                    Apellido Materno:</label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Left" Width="30%">
                                <label style="font-weight: normal"  id="txtDetallMaterno" align="Center" runat="server" width="35%">
                                </label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Right" Width="20%">
                                <label id="detallDepto" runat="server">
                                    Departamento:</label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Left" Width="30%">
                                <label style="font-weight: normal"  id="txtDetallDepto" align="Center" runat="server" width="35%">
                                </label>
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="Right" Width="20%">
                                <label id="detallEmail" runat="server">
                                    Email:</label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Left" Width="30%">
                                <label style="font-weight: normal"  id="txtDetallEmail" align="Center" runat="server" width="35%">
                                </label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Right" Width="20%">
                                <label id="detallEmpresa" runat="server">
                                    Empresa:</label>
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Left" Width="30%">
                                <label style="font-weight: normal"  id="txtDetallEmpresa" align="Center" runat="server" width="35%">
                                </label>
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                    <br>
                    <asp:Table ID="Table2" runat="server" CellPadding="5" Width="70%" HorizontalAlign="Center">
                        <asp:TableHeaderRow BackColor="#2E6E9E" ForeColor="White">
                            <asp:TableCell HorizontalAlign="Center" Width="30%">Extensiones
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Center" Width="40%">Sitios
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Center" Width="30%">Radio
                            </asp:TableCell>
                        </asp:TableHeaderRow>
                        <asp:TableRow Visible="false" ID="r1" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow Visible="false" ID="r2" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow Visible="false" ID="r3" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow Visible="false" ID="r4" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow Visible="false" ID="r5" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow Visible="false" ID="r6" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow Visible="false" ID="r7" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow Visible="false" ID="r8" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow Visible="false" ID="r9" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow Visible="false" ID="r10" ForeColor="Black">
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                            <asp:TableCell></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="Center" ColumnSpan="3">
                                <br>
                                <br>
                                <asp:Button ID="btnBuscarOtro" runat="server" Text="Buscar Otro" OnClick="btnBuscarOtro_Click" />
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                </asp:PlaceHolder>
            </div>
        </div>
    </form>
</body>
</html>
