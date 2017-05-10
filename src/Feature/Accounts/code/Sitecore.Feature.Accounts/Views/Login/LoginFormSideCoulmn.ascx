<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginFormSideCoulmn.ascx.cs" Inherits="Sitecore.Website.Controls.Login.LoginFormSideCoulmn" %>
   
   <div id="login-col">
        	<h3>Login</h3>
            <ul>
            	<li>
                	<label for="loginid">Login ID</label>
                	<input type="text" name="loginid" />
                    <a href="#">Forgot your Password?</a>                </li> 
                <li>
                	<label for="loginid">Password</label>
                	<input type="text" name="loginid" />                    
                    <a href="#">Forgot your Login ID</a>                </li>      
            </ul>    
            
            <div id="submit-area">
                <input class="btn-login" type="submit" value="" />
                <div class="remember">
                    <input type="checkbox" name="remember" id="remember" /><label for="remember">Remember Me</label>
                </div>
            </div>     
            <p><a href="/registration.aspx">Register Now</a> to join the fun!</p>
        </div> 