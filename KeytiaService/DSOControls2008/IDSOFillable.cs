/*
Nombre:		    JCMS
Fecha:		    2011-03-23
Descripción:	Interface para identificar los controles que se pueden llenar mediante un DataSource
Modificación:	2011-04-01 Se agrego interface para identificar a los controles simples con un unico valor
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace DSOControls2008
{
    public delegate void AfterFillEventHandler(object sender, EventArgs e);

    public interface IDSOFillable
    {
        object DataSource { get; set; }
        void Fill();
        event AfterFillEventHandler AfterFill;
    }

    public interface IDSOFillableInput : IDSOFillable
    {
        TextBox TextValue { get; }
    }
}
