<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AppCCustodia.aspx.cs" Inherits="CCustodiaDTIExt.CCustodia.AppCCustodia" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>Keytia</title>
    <link rel="SHORTCUT ICON" href="../img/favicon.ico" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <meta content="" name="description" />
    <meta content="" name="author" />

    <link href="https://fonts.googleapis.com/css?family=Poppins:300,300i,400,400i,500,500i,700,700i" rel="stylesheet" />
    <link href="../Styles/scripts/assets/global/plugins/bootstrap/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="../Styles/scripts/assets/global/css/components.min.css" rel="stylesheet" type="text/css" />
    <link href="../Styles/scripts/assets/global/css/plugins.min.css" rel="stylesheet" type="text/css" />
    <link href="../Styles/scripts/assets/layouts/layout/css/layout.min.css" rel="stylesheet" type="text/css" />
    <link href="../Styles/scripts/assets/layouts/layout/css/themes/darkblue.min.css" rel="stylesheet" type="text/css" />
    <link href="../Styles/scripts/assets/global/plugins/FontAwesome/css/font-awesome.css" rel="stylesheet" />
    <link href="../Styles/scripts/assets/global/plugins/bootstrap-select/css/bootstrap-select.min.css" rel="stylesheet" />

    <link href="../Styles/css/keytia.css" rel="stylesheet" type="text/css" />
</head>
<body class="page-header-fixed page-sidebar-closed-hide-logo page-content-white page-sidebar-closed">
    <form id="form1" runat="server">
        <div class="page-content">
            <div style="padding: 20px">
                <!--Script Manager para AjaxControlToolkit-->
                <asp:ToolkitScriptManager ID="tsmAjaxControls" runat="server" EnableScriptLocalization="true"
                    EnableScriptGlobalization="true">
                </asp:ToolkitScriptManager>

                <asp:Label ID="Label1" runat="server" Text="Carta Custodia" CssClass="page-title-keytia"></asp:Label>

                <asp:Table ID="tblHeaderCCustodia" runat="server" Width="100%">
                    <asp:TableRow ID="tblrHeaderCCustodia" runat="server">
                        <asp:TableCell ID="tblcTitulo" runat="server" HorizontalAlign="Left">
                        </asp:TableCell>
                        <asp:TableCell ID="TableCell1" runat="server" HorizontalAlign="Right">
                            <asp:LinkButton ID="lbtnRegresarPagBusqExternaCCust" runat="server" Text="Volver a los resultados de la búsqueda"
                                Font-Bold="true" Visible="False" OnClick="lbtnRegresarPagBusqExternaCCust_Click"></asp:LinkButton>
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
                <br />
                <!--Datos de Empleado-->
                <asp:Panel ID="pHeaderDatosEmple" runat="server" CssClass="collapsible-Keytia">
                    <asp:Table ID="tblHeaderEmple" runat="server" Width="100%">
                        <asp:TableRow ID="tblHeaderEmpleF1" runat="server">
                            <asp:TableCell ID="tblHeaderEmpleC1" runat="server">
                                <asp:Label ID="lblDatosEmple" runat="server" Text="Datos de Empleado"></asp:Label>
                            </asp:TableCell>
                            <asp:TableCell ID="tblHeaderEmpleC2" runat="server" HorizontalAlign="Right">
                                <asp:Image ID="imgExpandCollapse" runat="server" ImageAlign="Middle" Style="cursor: pointer" />
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                </asp:Panel>
                <asp:Panel ID="pDatosEmple" runat="server" BackColor="White" CssClass="col-md-12 col-sm-12">
                    <asp:UpdatePanel ID="upDatosEmple" UpdateMode="Conditional" runat="server">
                        <ContentTemplate>
                            <br />
                            <div class="form-horizontal" role="form">
                                <div class="row">
                                    <div class="col-md-6 col-sm-6">
                                        <div class="form-group">
                                            <asp:Label ID="lblFechaT" runat="server" CssClass="col-sm-4 control-label">Fecha:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtFecha" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                                <asp:HiddenField ID="hdnFechaFinEmple" runat="server" />
                                                <asp:CalendarExtender ID="ceSelectorFecha1" runat="server" TargetControlID="txtFecha">
                                                </asp:CalendarExtender>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblNominaT" runat="server" CssClass="col-sm-4 control-label">Nómina:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtNominaEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblNombreT" runat="server" CssClass="col-sm-4 control-label">Nombre:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtNombreEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblPaternoT" runat="server" CssClass="col-sm-4 control-label">Apellido Paterno:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtApPaternoEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblUbicacionT" runat="server" CssClass="col-sm-4 control-label">Ubicación:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="drpSitioEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblEmpresa" runat="server" CssClass="col-sm-4 control-label">Empresa:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="drpEmpresaEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblCenCos" runat="server" CssClass="col-sm-4 control-label">Centro de costos:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="drpCenCosEmple" runat="server" Enabled="false" ReadOnly="true" CssClass="form-control">
                                                </asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblLocalildadT" runat="server" CssClass="col-sm-4 control-label">Localidad:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="drpLocalidadEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblJefeInmT" runat="server" CssClass="col-sm-4 control-label">Jefe Inmediato:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="drpJefeEmple" runat="server" Enabled="false" ReadOnly="true" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblEmailJefe" runat="server" CssClass="col-sm-4 control-label">E-mail del jefe:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtEmailJefeEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-6 col-sm-6">
                                        <div class="form-group">
                                            <asp:Label ID="lblFolioT" runat="server" CssClass="col-sm-4 control-label">No. de Folio:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtFolioCCustodia" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <asp:Panel ID="pnlEstatus" runat="server" CssClass="form-group">
                                            <asp:Label ID="lblStatusT" runat="server" CssClass="col-sm-4 control-label">Estatus:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtEstatusCCustodia" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </asp:Panel>
                                        <div class="form-group">
                                            <asp:Label ID="lblSegundoNomT" runat="server" CssClass="col-sm-4 control-label">Segundo Nombre:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtSegundoNombreEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblMaternoT" runat="server" CssClass="col-sm-4 control-label">Apellido Materno:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtApMaternoEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblTipoEmpleT" runat="server" CssClass="col-sm-4 control-label">Tipo de Empleado:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="drpTipoEmpleado" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblPuestoT" runat="server" CssClass="col-sm-4 control-label">Puesto:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="drpPuestoEmple" runat="server" Enabled="false" ReadOnly="true" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblEmailEmple" runat="server" CssClass="col-sm-4 control-label">E-mail:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtEmailEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label ID="lblUusarT" runat="server" CssClass="col-sm-4 control-label">Usuario:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtUsuarRedEmple" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <div class="col-sm-offset-4 col-sm-3">
                                                <asp:CheckBox ID="cbEsGerenteEmple" runat="server" Text="Gerente" Enabled="false" Checked="false" CssClass="checkbox-inline" />
                                            </div>
                                            <div class="col-sm-5">
                                                <asp:CheckBox ID="cbVisibleDirEmple" runat="server" Text="Visible en directorio" Enabled="false" Checked="false" CssClass="checkbox-inline" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <br />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </asp:Panel>
                <asp:CollapsiblePanelExtender ID="cpeDatosEmpleImg" runat="server" TargetControlID="pDatosEmple"
                    ExpandControlID="pHeaderDatosEmple" CollapseControlID="pHeaderDatosEmple" CollapsedText="Mostrar..."
                    ExpandedText="Ocultar" ImageControlID="imgExpandCollapse" ExpandedImage="~/images/up-arrow-square-blue.png"
                    CollapsedImage="~/images/down-arrow-square-blue.png" ExpandDirection="Vertical">
                </asp:CollapsiblePanelExtender>
                <br />
                <!--Datos de Inventario-->
                <!--20170620 Se retira la parte de Inventarios y Politicas a Peticion de RJ-->
                <%--<asp:Panel ID="pHeaderInventario" runat="server" CssClass="headerCCustodia">
            <asp:Table ID="tblHeaderInventario" runat="server" Width="100%">
                <asp:TableRow ID="tblHeaderInventarioF1" runat="server">
                    <asp:TableCell ID="tblHeaderInventarioC1" runat="server">
                        <asp:Label ID="lblInventario" runat="server" CssClass="titleSeccionCCustodia" Text="Inventario asignado"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell ID="tblHeaderInventarioC2" runat="server" HorizontalAlign="Right">
                        <asp:Image ID="imgExpandCollapse2" runat="server" ImageAlign="Middle" Style="cursor: pointer" /></asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </asp:Panel>
        <asp:Panel ID="pDatosInventario" runat="server">
            <asp:UpdatePanel ID="upDatosInventario" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:GridView ID="grvInventario" runat="server" CellPadding="4" CssClass="GridView"
                        DataKeyNames="iCodMarca,iCodModelo" GridLines="None" AutoGenerateColumns="false"
                        ShowFooter="true" Width="100%" Style="text-align: center; margin-top: 0px;" EmptyDataText="No existe inventario asignado a este empleado">
                        <Columns>
                            <asp:BoundField DataField="Marca" HeaderText="Marca" HtmlEncode="true" />
                            <asp:BoundField DataField="Modelo" HeaderText="Modelo" HtmlEncode="true" />
                            <asp:BoundField DataField="TipoAparato" HeaderText="Tipo de Aparato" HtmlEncode="true" />
                            <asp:BoundField DataField="NoSerie" HeaderText="No. de Serie" HtmlEncode="true" />
                            <asp:BoundField DataField="MACAddress" HeaderText="MAC Address" HtmlEncode="true" />
                            <asp:BoundField DataField="iCodMarca" HtmlEncode="true" Visible="false" />
                            <asp:BoundField DataField="iCodModelo" HtmlEncode="true" Visible="false" />
                        </Columns>
                        <RowStyle CssClass="GridRowOdd" />
                        <AlternatingRowStyle CssClass="GridRowEven" />
                    </asp:GridView>
                    <br />
                    <br />                    
                </ContentTemplate>
                <Triggers>
                    <asp:PostBackTrigger ControlID="grvInventario" />
                </Triggers>
            </asp:UpdatePanel>
        </asp:Panel>
        <asp:CollapsiblePanelExtender ID="cpeInventario" runat="server" TargetControlID="pDatosInventario"
            ExpandControlID="pHeaderInventario" CollapseControlID="pHeaderInventario" CollapsedText="Mostrar..."
            ExpandedText="Ocultar" ImageControlID="imgExpandCollapse2" ExpandedImage="~/images/up-arrow-square-orange.png"
            CollapsedImage="~/images/down-arrow-square-orange.png" ExpandDirection="Vertical">
        </asp:CollapsiblePanelExtender>--%>
                <br />
                <!--Datos de códigos, extensiones y lineas-->
                <asp:Panel ID="pHeaderCodAutoExten" runat="server" CssClass="collapsible-Keytia">
                    <asp:Table ID="tblHeaderCodAutoExten" runat="server" Width="100%">
                        <asp:TableRow ID="tblHeaderCodAutoExtenF1" runat="server">
                            <asp:TableCell ID="tblHeaderCodAutoExtenC1" runat="server">
                                <asp:Label ID="lblCodAutoExten" runat="server" Text="Recursos asignados"></asp:Label>
                            </asp:TableCell>
                            <asp:TableCell ID="tblHeaderCodAutoExtenC2" runat="server" HorizontalAlign="Right">
                                <asp:Image ID="imgExpandCollapse3" runat="server" ImageAlign="Middle" Style="cursor: pointer" />
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                </asp:Panel>
                <asp:Panel ID="pDatosCodAutoExten" runat="server" CssClass="col-md-12 col-sm-12" BackColor="White">
                    <div class="form-horizontal" role="form">
                        <br />
                        <!--Extensiones-->
                        <asp:UpdatePanel ID="upDatosCodAutoExten" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="table-responsive">
                                    <asp:GridView ID="grvExten" runat="server" DataKeyNames="Exten,Sitio,TipoExten,iCodRegRelEmpExt"
                                        AutoGenerateColumns="false" HeaderStyle-CssClass="tableHeaderStyle" CssClass="table table-bordered"
                                        EmptyDataText="No existen extensiones asignadas a este empleado" Font-Size="Medium">
                                        <Columns>
                                            <%--AM. 20130717 Se agregaron las columnas de FechaFin y numero de registro de la relación--%>
                                            <%--0--%><asp:BoundField DataField="Exten" Visible="false" ReadOnly="true" />
                                            <%--1--%><asp:BoundField DataField="Sitio" Visible="false" ReadOnly="true" />
                                            <%--2--%><asp:BoundField DataField="TipoExten" Visible="false" ReadOnly="true" />
                                            <%--3--%><asp:BoundField DataField="ExtenCod" HeaderText="Extensión" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--4--%><asp:BoundField DataField="SitioDesc" HeaderText="Sitio" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--5--%><asp:BoundField DataField="FechaIni" HeaderText="Fecha Inicial" HtmlEncode="true" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                                            <%--6--%><asp:BoundField DataField="FechaFin" HeaderText="Fecha Final" HtmlEncode="true" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                                            <%--7--%><asp:BoundField DataField="TipoExtenDesc" HeaderText="Tipo" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--8--%><asp:CheckBoxField DataField="VisibleDir" HeaderText="Visible en Directorio" ItemStyle-HorizontalAlign="Center" />
                                            <%--9--%><asp:BoundField DataField="ComentarioExten" HeaderText="Comentarios" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--10--%><asp:BoundField DataField="iCodRegRelEmpExt" Visible="false" ReadOnly="true" />
                                        </Columns>
                                    </asp:GridView>
                                    <br />
                                </div>
                                <br />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <!--Códigos-->
                        <asp:UpdatePanel ID="upDatosCodAutoExten2" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="table-responsive">
                                    <asp:GridView ID="grvCodAuto" runat="server" DataKeyNames="CodAuto,Sitio,Cos,iCodRegRelEmpCodAuto"
                                        AutoGenerateColumns="false" HeaderStyle-CssClass="tableHeaderStyle" CssClass="table table-bordered"
                                        EmptyDataText="No existen códigos asignados a este empleado">
                                        <Columns>
                                            <%--0--%><asp:BoundField DataField="CodAutoCod" HeaderText="Código de Llamadas" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--1--%><asp:BoundField DataField="SitioDesc" HeaderText="Sitio" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--2--%><asp:BoundField DataField="CosDesc" HeaderText="Cos" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--3--%><asp:BoundField DataField="FechaIni" HeaderText="Fecha Inicial" HtmlEncode="true" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                                            <%--4--%><asp:BoundField DataField="FechaFin" HeaderText="Fecha Fin" HtmlEncode="true" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                                            <%--5--%><asp:BoundField DataField="CodAuto" HtmlEncode="true" Visible="false" />
                                            <%--6--%><asp:BoundField DataField="Sitio" HtmlEncode="true" Visible="false" />
                                            <%--7--%><asp:BoundField DataField="Cos" HtmlEncode="true" Visible="false" />
                                            <%--8--%><asp:BoundField DataField="iCodRegRelEmpCodAuto" Visible="false" ReadOnly="true" />
                                        </Columns>
                                    </asp:GridView>
                                    <br />
                                </div>
                                <br />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <!--Lineas-->
                        <asp:UpdatePanel ID="UpDatosLinea" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="table-responsive">
                                    <asp:GridView ID="grvLinea" runat="server" DataKeyNames="Linea,Carrier,Sitio,iCodRegRelEmpLinea"
                                        AutoGenerateColumns="false" HeaderStyle-CssClass="tableHeaderStyle" CssClass="table table-bordered"
                                        EmptyDataText="No existen lineas asignadas a este empleado">
                                        <Columns>
                                            <%--0--%><asp:BoundField DataField="LineaCod" HeaderText="Línea" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--1--%><asp:BoundField DataField="CarrierDesc" HeaderText="Carrier" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--2--%><asp:BoundField DataField="SitioDesc" HeaderText="Sitio" HtmlEncode="true" ItemStyle-HorizontalAlign="Center" />
                                            <%--3--%><asp:BoundField DataField="FechaIni" HeaderText="Fecha Inicial" HtmlEncode="true" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                                            <%--4--%><asp:BoundField DataField="FechaFin" HeaderText="Fecha Fin" HtmlEncode="true" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Center" />
                                            <%--5--%><asp:BoundField DataField="Linea" HtmlEncode="true" Visible="false" />
                                            <%--6--%><asp:BoundField DataField="Carrier" HtmlEncode="true" Visible="false" />
                                            <%--7--%><asp:BoundField DataField="Sitio" HtmlEncode="true" Visible="false" />
                                            <%--8--%><asp:BoundField DataField="iCodRegRelEmpLinea" Visible="false" ReadOnly="true" />
                                        </Columns>
                                    </asp:GridView>
                                    <br />
                                </div>
                                <br />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </asp:Panel>
                <asp:CollapsiblePanelExtender ID="cpeCodAutoExten" runat="server" TargetControlID="pDatosCodAutoExten"
                    ExpandControlID="pHeaderCodAutoExten" CollapseControlID="pHeaderCodAutoExten"
                    CollapsedText="Mostrar..." ExpandedText="Ocultar" ImageControlID="imgExpandCollapse3"
                    ExpandedImage="~/images/up-arrow-square-blue.png" CollapsedImage="~/images/down-arrow-square-blue.png"
                    ExpandDirection="Vertical">
                </asp:CollapsiblePanelExtender>
                <br />
                <br />
                <!--Comentarios-->
                <asp:Panel ID="pComentarios" runat="server" CssClass="row">
                    <div class="col-md-12 col-sm-12">
                        <div class="portlet solid bordered">
                            <div class="portlet-title">
                                <div class="caption">
                                    <i class="icon-bar-chart font-dark hide"></i>
                                    <span class="caption-subject titlePortletKeytia">Cometarios</span>
                                </div>                                
                            </div>
                            <div class="portlet-body">
                                <div id="RepComentariosCollapse" class="form-horizontal" role="form">
                                    <div class="row">
                                        <div class="col-md-6 col-sm-6">
                                            <asp:Label runat="server" CssClass="control-label">Comentarios del administrador:</asp:Label>
                                            <asp:TextBox ID="txtComentariosAdmin" runat="server" TextMode="MultiLine" Height="50px" CssClass="form-control"></asp:TextBox>
                                        </div>
                                        <div class="col-md-6 col-sm-6">
                                            <asp:Label runat="server" CssClass="control-label">Comentarios del empleado:</asp:Label>
                                            <asp:TextBox ID="txtComenariosEmple" runat="server" TextMode="MultiLine" Height="50px" CssClass="form-control"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="tblFechasCC" runat="server" CssClass="row">
                    <div class="col-md-12 col-sm-12">
                        <div class="portlet solid bordered">
                            <br />
                            <div class="form-horizontal" role="form">
                                <div class="row">
                                    <div class="col-md-6 col-sm-6">
                                        <div class="form-group">
                                            <asp:Label runat="server" CssClass="col-sm-4 control-label">Última modificación:</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtUltimaMod" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <asp:Label runat="server" CssClass="col-sm-4 control-label">Último envío</asp:Label>
                                            <div class="col-sm-8">
                                                <asp:TextBox ID="txtUltimoEnvio" runat="server" ReadOnly="true" Enabled="false" CssClass="form-control"></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-6 col-sm-6">
                                        <div class="form-group">
                                            <div class="col-sm-12">
                                                <asp:LinkButton ID="btnCambiarEstatusPte" runat="server" OnClick="btnCambiarEstatusPte_Click" Text="Cambiar a estatus PENDIENTE" CssClass="btn btn-keytia-sm pull-right" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <!--Carta Custodia Emple-->
                <asp:Table ID="tblEmpleCCust" runat="server" Width="100%" Visible="False">
                    <asp:TableRow ID="trEmpleCCust1" runat="server">
                        <asp:TableCell ID="tcEmpleCCust1" runat="server" HorizontalAlign="Right">
                            <asp:Button ID="btnAceptarCCust" runat="server" Text="Aceptar" OnClick="btnAceptarCCust_Click" CssClass="btn btn-keytia-sm" Style="margin-left: 20px" />
                        </asp:TableCell>
                        <asp:TableCell ID="tcEmpleCCust2" runat="server" HorizontalAlign="Left">
                            <asp:Button ID="btnRechazarCCust" runat="server" Text="Rechazar" OnClick="btnRechazarCCust_Click" CssClass="btn btn-keytia-sm" Style="margin-left: 20px" />
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
                <!--Exportar a PDF-->
                <asp:ImageButton ID="imgbPDFExport" runat="server" ImageUrl="~/images/adobe-pdf-logo.png"
                    Width="4%" OnClick="imgbPDFExport_Click" />
                <asp:AlwaysVisibleControlExtender ID="avcePDFExport" runat="server" TargetControlID="imgbPDFExport"
                    VerticalSide="Top" VerticalOffset="20" HorizontalSide="Right" HorizontalOffset="20"
                    ScrollEffectDuration=".1" />

                <!--Modal PopUp para aviso de empleado en aceptar o rechazar ccustodia-->
                <asp:Panel ID="pnlNotificaEmple" runat="server" TabIndex="-1" role="dialog" CssClass="modal-Keytia" Style="display: none;">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <asp:Label ID="Label2" runat="server" Text="Carta custodia"></asp:Label>
                                <button type="button" class="close" data-dismiss="modal" aria-hidden="true" id="btnCerrarAceptaORechaza"><i class="fas fa-times"></i></button>
                            </div>
                            <div class="modal-body">
                                <asp:Label Font-Bold="true" ID="lblMensajeNotificaEmple1" runat="server"></asp:Label>
                            </div>
                            <div class="modal-footer">
                                <asp:Button ID="btnNotificaEmpleCCust" runat="server" Text="Aceptar" CssClass="btn btn-keytia-sm" OnClientClick="return Hidepopup()" />
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:LinkButton ID="lnkFakeNotificaEmple" runat="server"></asp:LinkButton>
                <asp:ModalPopupExtender ID="mpeNotificaEmple" runat="server" DropShadow="false" PopupControlID="pnlNotificaEmple"
                    TargetControlID="lnkFakeNotificaEmple" BackgroundCssClass="modalPopupBackground" CancelControlID="btnCerrarAceptaORechaza">
                </asp:ModalPopupExtender>

                <!--Modal Popup para la confirmación de la baja dele empleado-->
                <asp:Panel ID="pnlBajaEmple" runat="server" TabIndex="-1" role="dialog" CssClass="modal-Keytia" Style="display: none;">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <asp:Label runat="server" Text="Baja de Empleado"></asp:Label>
                                <button type="button" class="close" data-dismiss="modal" aria-hidden="true" id="btnCerrarBajaEmple"><i class="fas fa-times"></i></button>
                            </div>
                            <div class="modal-body">
                                <div class="row">
                                    <div class="form-horizontal" role="form">
                                        <div class="col-md-12 col-sm-12">
                                            <asp:Label ID="lcEmpleEnBajaMsj" runat="server"></asp:Label>
                                            <div class="form-group">
                                                <asp:Label ID="lblFechaBajaEmple" runat="server" Text="Fecha Fin:" CssClass="col-sm-4 control-label"></asp:Label>
                                                <div class="col-sm-5">
                                                    <asp:TextBox ID="txtFechaBajaEmpleado" runat="server" MaxLength="10" ReadOnly="false" CssClass="form-control"></asp:TextBox>
                                                    <asp:CalendarExtender ID="ceFechaBajaEmple" TargetControlID="txtFechaBajaEmpleado" runat="server">
                                                    </asp:CalendarExtender>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="modal-footer">
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:LinkButton ID="lnkFakeBajaEmple" runat="server"></asp:LinkButton>
                <asp:ModalPopupExtender ID="mpeBajaEmple" runat="server" DropShadow="false" PopupControlID="pnlBajaEmple"
                    TargetControlID="lnkFakeBajaEmple" BackgroundCssClass="modalPopupBackground" CancelControlID="btnCerrarBajaEmple">
                </asp:ModalPopupExtender>

                <%--NZ: Modal para mensajes--%>
                <asp:Panel ID="pnlPopupMensaje" runat="server" TabIndex="-1" role="dialog" CssClass="modal-Keytia" Style="display: none;">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <asp:Label ID="lblTituloModalMsn" runat="server" Text=""></asp:Label>
                                <button type="button" class="close" data-dismiss="modal" aria-hidden="true" id="btnCerrarMensajes"><i class="fas fa-times"></i></button>
                            </div>
                            <div class="modal-body">
                                <asp:Label ID="lblBodyModalMsn" runat="server" Text=""></asp:Label>
                            </div>
                            <div class="modal-footer">
                                <asp:Button ID="btnYes" runat="server" Text="OK" CssClass="btn btn-keytia-sm" />
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:LinkButton ID="lnkBtnMsn" runat="server" Style="display: none"></asp:LinkButton>
                <asp:ModalPopupExtender ID="mpeEtqMsn" runat="server" PopupControlID="pnlPopupMensaje"
                    TargetControlID="lnkBtnMsn" OkControlID="btnYes" BackgroundCssClass="modalPopupBackground" CancelControlID="btnCerrarMensajes">
                </asp:ModalPopupExtender>
            </div>
        </div>
    </form>
</body>
</html>
