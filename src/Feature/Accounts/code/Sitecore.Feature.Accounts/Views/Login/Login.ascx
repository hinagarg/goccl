<%@ Control Language="c#" AutoEventWireup="true" Inherits="Sitecore.Website.Controls.Login.Login"
    CodeBehind="Login.ascx.cs" %>

<asp:Panel ID="pnlLogin" runat="server" Visible="true" DefaultButton="lnkLogin">

<div id="login-col">
    <h3>
        Login</h3>
    <ul>
        <li>
        <asp:Label ID="LblMessage" runat="server" ForeColor="Red"></asp:Label>
            <label for="loginid">
                Login ID</label><asp:RequiredFieldValidator ID="ReqLoginId" runat="server" Display="Dynamic"
                    ErrorMessage="Login Id is Required" ControlToValidate="txtLogin" ValidationGroup="A"></asp:RequiredFieldValidator>
            <asp:RegularExpressionValidator ID="RevLoginId" runat="server" ControlToValidate="txtLogin"
                ErrorMessage="Login id does not contain spaces,double quotes or +&gt;&lt;=';:,|/"
                ValidationExpression="[^ &quot;+&gt;&lt;=':,|/]*$" ValidationGroup="A" Display="Dynamic"></asp:RegularExpressionValidator>
            <input type="text" name="loginid" id="txtLogin" runat="server" tabindex="1" />
            <a href="/home/Forgotpassword.aspx">Forgot your Login ID</a> 
        </li>
        <li>
            <label for="loginid">
                Password</label>
            <asp:RequiredFieldValidator ID="ReqPassword" runat="server" ErrorMessage="Password is Required"
                ControlToValidate="txtPass" Display="Dynamic" ValidationGroup="A"></asp:RequiredFieldValidator>
            <asp:RegularExpressionValidator ID="RevPassword" runat="server" ControlToValidate="txtPass"
                ErrorMessage="Space is not allowed" ValidationExpression="[^ ]*$" ValidationGroup="A"
                Display="Dynamic"></asp:RegularExpressionValidator>
            <input name="loginid" id="txtPass" runat="server" type="password" tabindex="2"/>
            <a href="/home/Forgotpassword.aspx">Forgot your Password?</a>
        </li>
    </ul>
    <div id="submit-area">
   
        <asp:LinkButton ID="lnkLogin" runat="server" CssClass="button"  ValidationGroup="A" OnClick="ButtonLogin_Click"><span><%=ButttonText%></span></asp:LinkButton>
        <div class="remember">
            <input type="checkbox" name="remember" id="remember" runat="server" /><label for="remember">Remember
                Me</label>
        </div>
    </div>
    <p><a href="<%=RegistrationUrl %>"><%=RegisterButttonText %></a> <%=RegisterGreeting %></p>
</div>
</asp:Panel>