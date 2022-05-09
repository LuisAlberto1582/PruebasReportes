<%@ Page Title="" Language="C#" AutoEventWireup="true"
    CodeBehind="AppCCustodia.aspx.cs" Inherits="AppExtNextel.CCustodia.AppCCustodia"
    ValidateRequest="true" EnableEventValidation="false" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title></title>
<!--Scripts en jQuery-->

<%--    <script src="../../scripts/jquery.blockUI.js" type="text/javascript"></script>

    <script src="../../scripts/jQueryBlockUIPopUp.js" type="text/javascript"></script>
--%>
    <script type="text/javascript" language="javascript">
        
        
        function alerta(mensaje) {
            alert( mensaje);
        }

        
        
    </script>
    <link href="../CCustodia.css" rel="stylesheet" type="text/css" />    
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    <!--Script Manager para AjaxControlToolkit-->
    <asp:ToolkitScriptManager ID="tsmAjaxControls" runat="server" EnableScriptLocalization="true"
        EnableScriptGlobalization="true">
    </asp:ToolkitScriptManager>
    <asp:Table ID="tblHeaderCCustodia" runat="server" Width="100%">
        <asp:TableRow ID="tblrHeaderCCustodia" runat="server">
            <asp:TableCell ID="tblcTitulo" runat="server" HorizontalAlign="Left">
                <asp:Label ID="lblTitle" runat="server" Text="Cartas Custodia Servicios de Voz" CssClass="titleCCustodia"></asp:Label>
            </asp:TableCell>
            <asp:TableCell ID="TableCell1" runat="server" HorizontalAlign="Right">
                
                <asp:LinkButton ID="lbtnRegresarPagBusqExternaCCust" runat="server" Text="Volver a los resultados de la búsqueda"
                    Font-Bold="true" Visible="False" OnClick="lbtnRegresarPagBusqExternaCCust_Click"></asp:LinkButton>
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
    <br />
    <!--Datos de Empleado-->
    <asp:Panel ID="pHeaderDatosEmple" runat="server" CssClass="headerCCustodia">
        <asp:Table ID="tblHeaderEmple" runat="server" Width="100%">
            <asp:TableRow ID="tblHeaderEmpleF1" runat="server">
                <asp:TableCell ID="tblHeaderEmpleC1" runat="server">
                    <asp:Label ID="lblDatosEmple" runat="server" CssClass="titleSeccionCCustodia" Text="Datos de Empleado"></asp:Label>
                </asp:TableCell>
                <asp:TableCell ID="tblHeaderEmpleC2" runat="server" HorizontalAlign="Right">
                    <asp:Image ID="imgExpandCollapse" runat="server" ImageAlign="Middle" Style="cursor: pointer" /></asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <asp:Panel ID="pDatosEmple" runat="server">
        <asp:UpdatePanel ID="upDatosEmple" UpdateMode="Conditional" runat="server">
            
            <ContentTemplate>
                <br />
                <asp:Table ID="tblDatosEmple" runat="server" Width="100%">
                    <asp:TableRow ID="trFila0" runat="server">
                        <asp:TableCell ID="tcCelda01" runat="server">
Fecha:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda02" runat="server">
                            <asp:TextBox ID="txtFecha" runat="server" ReadOnly="false" Enabled="false" Width="250"></asp:TextBox>
                            <asp:HiddenField ID="hdnFechaFinEmple" runat="server" />
                            <asp:CalendarExtender ID="ceSelectorFecha1" runat="server" TargetControlID="txtFecha">
                            </asp:CalendarExtender>
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda03" runat="server">
No. de Folio:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda04" runat="server">
                            <asp:TextBox ID="txtFolioCCustodia" runat="server" ReadOnly="true" Enabled="false"
                                Width="250"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFila1" runat="server">
                        <asp:TableCell ID="tcCelda11" runat="server">
Estatus:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda12" runat="server">
                            <asp:TextBox ID="txtEstatusCCustodia" runat="server" ReadOnly="true" Enabled="false"
                                Width="250"></asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda13" runat="server">
Nómina:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda14" runat="server">
                            <asp:TextBox ID="txtNominaEmple" runat="server" ReadOnly="true" Enabled="false" Width="250"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFila2" runat="server">
                        <asp:TableCell ID="tcCelda21" runat="server">
Nombre:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda22" runat="server">
                            <asp:TextBox ID="txtNombreEmple" runat="server" ReadOnly="true" Enabled="false" Width="250"></asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcSegundoNombreC1" runat="server">
Segundo Nombre:
                        </asp:TableCell>
                        <asp:TableCell ID="tcSegundoNombreC2" runat="server">
                            <asp:TextBox ID="txtSegundoNombreEmple" runat="server" ReadOnly="true" Enabled="false"
                                Width="250"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFilaEmple" runat="server">
                        <asp:TableCell ID="tcApPaternoC1" runat="server">
Apellido Paterno:
                        </asp:TableCell>
                        <asp:TableCell ID="tcApPaternoC2" runat="server">
                            <asp:TextBox ID="txtApPaternoEmple" runat="server" ReadOnly="true" Enabled="false"
                                Width="250"></asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcApMaternoEmpleC1" runat="server">
Apellido Materno:
                        </asp:TableCell>
                        <asp:TableCell ID="tcApMaternoEmpleC2" runat="server">
                            <asp:TextBox ID="txtApMaternoEmple" runat="server" ReadOnly="true" Width="250" 
                                Enabled="false"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFilaEmple2" runat="server">
                        <asp:TableCell ID="tcCelda23" runat="server">
Ubicación:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda24" runat="server">
                            <asp:TextBox ID="drpSitioEmple" runat="server"  Enabled="false" ReadOnly="true" Width="250">
                               
                            </asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda81" runat="server">
Visible en directorio:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda82" runat="server">
                            <asp:CheckBox ID="cbVisibleDirEmple" Checked="false" runat="server" Enabled="false" /></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFilaEmpreUbica" runat="server">
                        <asp:TableCell ID="tcEmpresaC1" runat="server">
Empresa:
                        </asp:TableCell>
                        <asp:TableCell ID="tcEmpresaC2" runat="server">
                            <asp:TextBox ID="drpEmpresaEmple" runat="server" ReadOnly="true" Width="250"
                                Enabled="false">
                            </asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcTipoEmpleadoC1" runat="server">
Tipo de Empleado:
                        </asp:TableCell>
                        <asp:TableCell ID="tcTipoEmpleadoC2" runat="server">
                            <asp:TextBox ID="drpTipoEmpleado" runat="server" ReadOnly="true" Width="250"
                                Enabled="false">
                                
                            </asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFila3" runat="server">
                        <asp:TableCell ID="tcCelda31" runat="server">
Centro de costos:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda32" runat="server">
                            <asp:TextBox ID="drpCenCosEmple" runat="server" Enabled="false" ReadOnly="true" Width="250">
                                
                            </asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda33" runat="server">
Puesto:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda34" runat="server">
                            <asp:TextBox ID="drpPuestoEmple" runat="server" Enabled="false" ReadOnly="true" Width="250">
                               
                            </asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFila4" runat="server">
                        <asp:TableCell ID="tcCelda41" runat="server">
Localidad:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda42" runat="server">
                            <asp:TextBox ID="drpLocalidadEmple" runat="server" ReadOnly="true" Width="250"
                                Enabled="false">
                                
                            </asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda43" runat="server">
E-mail:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda44" runat="server">
                            <asp:TextBox ID="txtEmailEmple" runat="server" ReadOnly="true" Enabled="false" Width="250"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFila5" runat="server">
                        <asp:TableCell ID="tcCelda51" runat="server">
Radio AT&T:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda52" runat="server">
                            <asp:TextBox ID="txtRadioNextelEmple" runat="server" ReadOnly="false" Enabled="false"></asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda53" runat="server">
Attuid / Usuario de red:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda54" runat="server">
                            <asp:TextBox ID="txtUsuarRedEmple" runat="server" ReadOnly="true" Enabled="false"
                                Width="250"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFila6" runat="server">
                        <asp:TableCell ID="tcCelda61" runat="server">
Número Celular:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda62" runat="server">
                            <asp:TextBox ID="txtNumCelularEmple" runat="server" ReadOnly="true" Enabled="false"
                                Width="250"></asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda63" runat="server">
Gerente:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda64" runat="server">
                            <asp:CheckBox ID="cbEsGerenteEmple" Checked="false" Enabled="false" runat="server" /></asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow ID="trFila7" runat="server">
                        <asp:TableCell ID="tcCelda71" runat="server">
Jefe Inmediato:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda72" runat="server">
                            <asp:TextBox ID="drpJefeEmple" runat="server" Enabled="false" ReadOnly="true" Width="250">
                                
                            </asp:TextBox>
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda73" runat="server">
E-mail del jefe:
                        </asp:TableCell>
                        <asp:TableCell ID="tcCelda74" runat="server">
                            <asp:TextBox ID="txtEmailJefeEmple" runat="server" ReadOnly="true" Enabled="false"
                                Width="250"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
                <br />
                <!--RZ.20130718 Se agrega confirmbuttonextender y boton de cancelar-->
                
                
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
    <asp:Panel ID="pHeaderInventario" runat="server" CssClass="headerCCustodia">
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
                <!--Modal PopUp para inventario-->
                
                <asp:LinkButton ID="lnkFake" runat="server"></asp:LinkButton>
                <asp:ModalPopupExtender ID="mpeInventario" runat="server" DropShadow="false" PopupControlID="pnlAddEditInventario"
                    TargetControlID="lnkFake" BackgroundCssClass="modalBackground">
                </asp:ModalPopupExtender>
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
    </asp:CollapsiblePanelExtender>
    <br />
    <!--Datos de codigos y extensiones-->
    <asp:Panel ID="pHeaderCodAutoExten" runat="server" CssClass="headerCCustodia">
        <asp:Table ID="tblHeaderCodAutoExten" runat="server" Width="100%">
            <asp:TableRow ID="tblHeaderCodAutoExtenF1" runat="server">
                <asp:TableCell ID="tblHeaderCodAutoExtenC1" runat="server">
                    <asp:Label ID="lblCodAutoExten" runat="server" CssClass="titleSeccionCCustodia" Text="Recursos asignados"></asp:Label>
                </asp:TableCell>
                <asp:TableCell ID="tblHeaderCodAutoExtenC2" runat="server" HorizontalAlign="Right">
                    <asp:Image ID="imgExpandCollapse3" runat="server" ImageAlign="Middle" Style="cursor: pointer" />
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <asp:Panel ID="pDatosCodAutoExten" runat="server">
        <!--Extensiones-->
        <asp:UpdatePanel ID="upDatosCodAutoExten" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:GridView ID="grvExten" runat="server" CellPadding="4" CssClass="GridView" DataKeyNames="Exten,Sitio,TipoExten,iCodRegRelEmpExt"
                    GridLines="None" AutoGenerateColumns="false" ShowFooter="true" Width="100%" Style="text-align: center;
                    margin-top: 0px;" EmptyDataText="No existen extensiones asignadas a este empleado">
                    <Columns>
                        <%--AM. 20130717 Se agregaron las columnas de FechaFin y numero de registro de la relación--%>
                        <%--0--%><asp:BoundField DataField="Exten" Visible="false" ReadOnly="true" />
                        <%--1--%><asp:BoundField DataField="Sitio" Visible="false" ReadOnly="true" />
                        <%--2--%><asp:BoundField DataField="TipoExten" Visible="false" ReadOnly="true" />
                        <%--3--%><asp:BoundField DataField="ExtenCod" HeaderText="Extensión" HtmlEncode="true" />
                        <%--4--%><asp:BoundField DataField="SitioDesc" HeaderText="Sitio" HtmlEncode="true" />
                        <%--5--%><asp:BoundField DataField="FechaIni" HeaderText="Fecha Inicial" HtmlEncode="true"
                            DataFormatString="{0:d}" />
                        <%--6--%><asp:BoundField DataField="FechaFin" HeaderText="Fecha Final" HtmlEncode="true"
                            DataFormatString="{0:d}" />
                        <%--7--%><asp:BoundField DataField="TipoExtenDesc" HeaderText="Tipo" HtmlEncode="true" />
                        <%--8--%><asp:CheckBoxField DataField="VisibleDir" HeaderText="Visible en Directorio" />
                        <%--9--%><asp:BoundField DataField="ComentarioExten" HeaderText="Comentarios" HtmlEncode="true" />
                        <%--10--%><asp:BoundField DataField="iCodRegRelEmpExt" Visible="false" ReadOnly="true" />
                        
                  
                    </Columns>
                    <RowStyle CssClass="GridRowOdd" />
                    <AlternatingRowStyle CssClass="GridRowEven" />
                </asp:GridView>
                <br />
                
                <br />
                <!--Modal PopUp para Extensiones-->
                
                <asp:LinkButton ID="lnkFakeExten" runat="server"></asp:LinkButton>
                <asp:ModalPopupExtender ID="mpeExten" runat="server" DropShadow="false" PopupControlID="pnlAddEditExten"
                    TargetControlID="lnkFakeExten" BackgroundCssClass="modalBackground">
                </asp:ModalPopupExtender>
            </ContentTemplate>
        </asp:UpdatePanel>
        <!--Codigos-->
        <asp:UpdatePanel ID="upDatosCodAutoExten2" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:GridView ID="grvCodAuto" runat="server" CellPadding="4" CssClass="GridView"
                    DataKeyNames="CodAuto,Sitio,iCodRegRelEmpCodAuto" GridLines="None" AutoGenerateColumns="false"
                    ShowFooter="true" Width="100%" Style="text-align: center; margin-top: 0px;" EmptyDataText="No existen códigos asignados a este empleado">
                    <Columns>
                        <%--0--%><asp:BoundField DataField="CodAutoCod" HeaderText="Código de Llamadas" HtmlEncode="true" />
                        <%--1--%><asp:BoundField DataField="SitioDesc" HeaderText="Sitio" HtmlEncode="true" />
                        <%--2--%><asp:BoundField DataField="FechaIni" HeaderText="Fecha Inicial" HtmlEncode="true"
                            DataFormatString="{0:d}" />
                        <%--3--%><asp:BoundField DataField="FechaFin" HeaderText="Fecha Fin" HtmlEncode="true"
                            DataFormatString="{0:d}" />
                        <%--4--%><asp:CheckBoxField DataField="VisibleDir" HeaderText="Visible en Directorio" />
                        <%--5--%><asp:BoundField DataField="CodAuto" HtmlEncode="true" Visible="false" />
                        <%--6--%><asp:BoundField DataField="Sitio" HtmlEncode="true" Visible="false" />
                        <%--7--%><asp:BoundField DataField="iCodRegRelEmpCodAuto" Visible="false" ReadOnly="true" />
                        
                        
                       
                    </Columns>
                    <RowStyle CssClass="GridRowOdd" />
                    <AlternatingRowStyle CssClass="GridRowEven" />
                </asp:GridView>
                <br />
                
                <br />
                <!--Modal PopUp para Codigos de Autorizacion-->
                
                <asp:LinkButton ID="lnkFakeCodAuto" runat="server"></asp:LinkButton>
                <asp:ModalPopupExtender ID="mpeCodAuto" runat="server" DropShadow="false" PopupControlID="pnlAddEditCodAuto"
                    TargetControlID="lnkFakeCodAuto" BackgroundCssClass="modalBackground">
                </asp:ModalPopupExtender>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
    <asp:CollapsiblePanelExtender ID="cpeCodAutoExten" runat="server" TargetControlID="pDatosCodAutoExten"
        ExpandControlID="pHeaderCodAutoExten" CollapseControlID="pHeaderCodAutoExten"
        CollapsedText="Mostrar..." ExpandedText="Ocultar" ImageControlID="imgExpandCollapse3"
        ExpandedImage="~/images/up-arrow-square-orange.png" CollapsedImage="~/images/down-arrow-square-orange.png"
        ExpandDirection="Vertical">
    </asp:CollapsiblePanelExtender>
    <br />
    
    <!--NZ 20150828 Se agrega sección para agregar usuarios a empleado. -->
    <!--Datos ID Usuario y PIN -->
    <asp:Panel ID="pHeaderUsuarios" runat="server" CssClass="headerCCustodia">
        <asp:Table ID="tblHeaderUsuarios" runat="server" Width="100%">
            <asp:TableRow ID="tblHeaderUsuariosF1" runat="server">
                <asp:TableCell ID="tblHeaderUsuariosC1" runat="server">
                    <asp:Label ID="lblUsuarios" runat="server" CssClass="titleSeccionCCustodia" Text="ID's de Usuario asignados"></asp:Label>
                </asp:TableCell>
                <asp:TableCell ID="tblHeaderUsuariosC2" runat="server" HorizontalAlign="Right">
                    <asp:Image ID="imgExpandCollapse5" runat="server" ImageAlign="Middle" Style="cursor: pointer" />
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <asp:Panel ID="pDatosUsuarios" runat="server">
    <!--Datos ID's Usuario-->
        <asp:UpdatePanel ID="UpDatosUsuarios" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:GridView ID="grvUsuarios" runat="server" CellPadding="4" CssClass="GridView"
                    DataKeyNames="iCodRegUsuario" GridLines="None" AutoGenerateColumns="false" ShowFooter="true"
                    Width="100%" Style="text-align: center; margin-top: 0px;" EmptyDataText="No existen ID's de usuario asignados a este empleado">
                    <Columns>
                        <%--0--%><asp:BoundField DataField="IdUsuario" HeaderText="ID de Usuario" HtmlEncode="true" />
                        <%--1--%><asp:BoundField DataField="Pin" HeaderText="PIN" HtmlEncode="true" />
                        <%--2--%><asp:BoundField DataField="FechaIni" HeaderText="Fecha Inicial" HtmlEncode="true"
                            DataFormatString="{0:d}" />
                        <%--3--%><asp:BoundField DataField="FechaFin" HeaderText="Fecha Fin" HtmlEncode="true"
                            DataFormatString="{0:d}" />
                        <%--4--%><asp:BoundField DataField="ComentariosUsuarios" HeaderText="Comentarios"
                            HtmlEncode="true" />
                        <%--5--%><asp:BoundField DataField="iCodRegUsuario" Visible="false" ReadOnly="true"
                            HtmlEncode="true" />                      
                     
                    </Columns>
                    <RowStyle CssClass="GridRowOdd" />
                    <AlternatingRowStyle CssClass="GridRowEven" />
                </asp:GridView>                
                <br />
                
                <br />                
                <!--Modal PopUp para IDs de Usuarios del Empleado-->
               
                <asp:LinkButton ID="lnkFakeUsuarios" runat="server"></asp:LinkButton>
                <asp:ModalPopupExtender ID="mpeUsuarios" runat="server" DropShadow="false" PopupControlID="pnlAddEditUsuarios"
                    TargetControlID="lnkFakeUsuarios" BackgroundCssClass="modalBackground">
                </asp:ModalPopupExtender>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
    <asp:CollapsiblePanelExtender ID="cpeUsuarios" runat="server" TargetControlID="pDatosUsuarios"
        ExpandControlID="pHeaderUsuarios" CollapseControlID="pHeaderUsuarios" CollapsedText="Mostrar..."
        ExpandedText="Ocultar" ImageControlID="imgExpandCollapse5" ExpandedImage="~/images/up-arrow-square-orange.png"
        CollapsedImage="~/images/down-arrow-square-orange.png" ExpandDirection="Vertical">
    </asp:CollapsiblePanelExtender>
    <br />
    <!--Comentarios-->
    <asp:Panel ID="pComentarios" runat="server">
        <asp:Table ID="tblComentarios" runat="server" Width="100%">
            <asp:TableRow ID="tblComentariosF1" runat="server">
                <asp:TableCell ID="tblComentariosC1" runat="server">
                    Comentarios del administrador:
                </asp:TableCell>
                <asp:TableCell ID="tblComentariosC2" runat="server">
                    Comentarios del empleado:
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="tblComentariosF2" runat="server">
                <asp:TableCell ID="tblComentariosC3" runat="server">
                    <asp:TextBox ID="txtComentariosAdmin" runat="server" TextMode="MultiLine" Width="50%"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell ID="tblComentariosC4" runat="server">
                    <asp:TextBox ID="txtComenariosEmple" runat="server" TextMode="MultiLine" Width="50%"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <br />
    <!--Politicas de Uso-->
    <asp:Panel ID="pPoliticasUso" runat="server" CssClass="headerCCustodia">
        <asp:Table ID="tblPoliticasUso" runat="server" Width="100%">
            <asp:TableRow ID="trPoliticasUso1" runat="server">
                <asp:TableCell ID="tcPoliticasUso1" runat="server">
                    <asp:Label ID="lblHeaderPoliticasUso" runat="server" CssClass="titleSeccionCCustodia"
                        Text="Politicas de Uso"></asp:Label>
                </asp:TableCell>
                <asp:TableCell ID="tcPoliticasUso2" runat="server" HorizontalAlign="Right">
                    <asp:Image ID="imgExpandCollapse4" runat="server" ImageAlign="Middle" Style="cursor: pointer" />
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <asp:Panel ID="pContentPoliticas" runat="server">
        <ul>
            <li>Es responsabilidad de la persona a la cual se le ha asignado un aparato telefónico
                con su código y extensión el buen uso de los mismos.</li>
            <li>Utilizar el teléfono como medio de comunicación para asuntos relacionados con lo
                laboral. </li>
            <li>La asignación del aparato telefónico queda bajo su custodia, siendo responsable
                del cuidado físico. </li>
            <li>Cualquier daño o pérdida de este equipo será cubierta por el usuario.</li>
            <li>El equipo no puede ser cambiado de lugar, modificar su configuración, instalar nuevo
                sw ni realizar cambio alguno sin la previa autorización del área de Telefonía Corporativa y/o Call Center.</li>
            <li>Telefonía Corporativa y/o Call Center estará encargado de controlar y vigilar el inventario de los equipos telefónicos
                y objetos asociados al área de telefonía por lo que cualquier actividad de mantenimiento
                preventivo o correctivo, cambios en la configuración o reporte de fallas en el funcionamiento
                deberá ser reportado en Service Desk.</li>
            <li>Está prohibido hacer uso de servicios de entretenimiento y cargos telefónicos.</li>
            <li>La asignación de las claves de acceso telefónico es personal e intransferible.</li>
            <li>El usuario tiene conocimiento que las llamadas que genere y reciba podrán ser monitoreadas.</li>
            <li>El usuario tiene conocimiento que los registros de las llamadas que generé y reciba
                podrán ser consultadas por él y por su jefe directo.</li>
            <li>El usuario tiene conocimiento que cualquier cambio o nuevo requerimiento de su servicio
                tendrá que ser a través de Service desk <asp:HyperLink runat="server" NavigateUrl="http://servicedesk.mx.att.com/SDPortal/" Text="http://servicedesk.mx.att.com/SDPortal/"></asp:HyperLink></li>
            <li>El no utilizar adecuadamente los recursos (teléfono, buzón, clave, etc.) será motivo
                de suspensión de los mismos.</li>
            <li>El usuario acepta que la información es correcta y aceptada por las políticas anteriores.</li>
        </ul>
        <asp:Table ID="tblFechasCC" runat="server">
            <asp:TableRow ID="trFechasCC1" runat="server">
                <asp:TableCell ID="tcFechasCC1" runat="server">
                Última modificación:
                </asp:TableCell>
                <asp:TableCell ID="tcFechasCC2" runat="server">
                    <asp:TextBox ID="txtUltimaMod" runat="server" ReadOnly="true" Enabled="false" Width="250"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="trFchasCC2" runat="server">
                <asp:TableCell ID="tcFechasCC3" runat="server">
                Último envío
                </asp:TableCell>
                <asp:TableCell ID="tcFechasCC4" runat="server">
                    <asp:TextBox ID="txtUltimoEnvio" runat="server" ReadOnly="true" Enabled="false" Width="250"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <asp:CollapsiblePanelExtender ID="cpePoliticasUso" runat="server" TargetControlID="pContentPoliticas"
        ExpandControlID="pPoliticasUso" CollapseControlID="pPoliticasUso" CollapsedText="Mostrar..."
        ExpandedText="Ocultar" ImageControlID="imgExpandCollapse4" ExpandedImage="~/images/up-arrow-square-orange.png"
        CollapsedImage="~/images/down-arrow-square-orange.png" ExpandDirection="Vertical">
    </asp:CollapsiblePanelExtender>
    <!--Editar Carta Custodia-->
    <asp:Table ID="tblEditCC" runat="server" Width="100%">
        <asp:TableRow ID="trEditCC1" runat="server">
            <asp:TableCell ID="tcEditCC1" runat="server" HorizontalAlign="Right">
                
            </asp:TableCell>
            <asp:TableCell ID="tcEditCC2" runat="server" HorizontalAlign="Left">
                <asp:Button ID="btnCambiarEstatusPte" runat="server" Text="Cambiar a estatus PENDIENTE"
                    OnClick="btnCambiarEstatusPte_Click" />
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
    <!--Carta Custodia Emple-->
    <asp:Table ID="tblEmpleCCust" runat="server" Width="100%" Visible="False">
        <asp:TableRow ID="trEmpleCCust1" runat="server">
            <asp:TableCell ID="tcEmpleCCust1" runat="server" HorizontalAlign="Right">
                <asp:Button ID="btnAceptarCCust" runat="server" Text="Aceptar" OnClick="btnAceptarCCust_Click" />
            </asp:TableCell>
            <asp:TableCell ID="tcEmpleCCust2" runat="server" HorizontalAlign="Left">
                <asp:Button ID="btnRechazarCCust" runat="server" Text="Rechazar" OnClick="btnRechazarCCust_Click" />
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
    <asp:Panel ID="pnlNotificaEmple" runat="server" CssClass="modalPopup" Style="display: none"
        Width="300" Height="100">
        <asp:Label Font-Bold="true" ID="lblMensajeNotificaEmple1" runat="server"></asp:Label>
        <asp:Label Font-Bold="true" ID="lblMensajeNotificaEmple2" runat="server"></asp:Label>
        <br />
        <br />
        <table align="center">
            <tr>
                <td>
                    <asp:Button ID="btnNotificaEmpleCCust" runat="server" Text="Aceptar" OnClientClick="return Hidepopup()" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:LinkButton ID="lnkFakeNotificaEmple" runat="server"></asp:LinkButton>
    <asp:ModalPopupExtender ID="mpeNotificaEmple" runat="server" DropShadow="false" PopupControlID="pnlNotificaEmple"
        TargetControlID="lnkFakeNotificaEmple" BackgroundCssClass="modalBackground">
    </asp:ModalPopupExtender>
    <!--Modal Popup para la confirmación de la baja dele empleado-->
    <asp:Panel ID="pnlBajaEmple" runat="server" CssClass="modalPopup" HorizontalAlign="Justify"
        Style="display: none" Width="500" Height="300">
        <asp:Literal ID="lcEmpleEnBajaMsj" runat="server"></asp:Literal>
        <asp:Panel ID="pnlBotonesBajaEmpleado" HorizontalAlign="Center" runat="server">
            <asp:Label ID="lblFechaBajaEmple" runat="server" Text="Fecha Fin: "></asp:Label>
            <asp:TextBox ID="txtFechaBajaEmpleado" runat="server"></asp:TextBox>
            <asp:CalendarExtender ID="ceFechaBajaEmple" TargetControlID="txtFechaBajaEmpleado"
                runat="server">
            </asp:CalendarExtender>
            <br />
            <br />
            
        </asp:Panel>
    </asp:Panel>
    <asp:LinkButton ID="lnkFakeBajaEmple" runat="server"></asp:LinkButton>
    <asp:ModalPopupExtender ID="mpeBajaEmple" runat="server" DropShadow="false" PopupControlID="pnlBajaEmple"
        TargetControlID="lnkFakeBajaEmple" BackgroundCssClass="modalBackground">
    </asp:ModalPopupExtender>
    <!--Modal Popup para la reasingacion del jefe inmediato-->
    
    <asp:LinkButton ID="lnkButtonFakeReasignaEmple" runat="server"></asp:LinkButton>
    
       
        </div>
    </form>
</body>
</html>