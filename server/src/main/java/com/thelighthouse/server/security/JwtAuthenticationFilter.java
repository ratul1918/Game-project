package com.thelighthouse.server.security;
import jakarta.servlet.FilterChain; import jakarta.servlet.ServletException; import jakarta.servlet.http.*;
import java.io.IOException; import org.springframework.security.authentication.UsernamePasswordAuthenticationToken; import org.springframework.security.core.authority.AuthorityUtils; import org.springframework.security.core.context.SecurityContextHolder; import org.springframework.web.filter.OncePerRequestFilter;
public class JwtAuthenticationFilter extends OncePerRequestFilter {
 private final JwtService jwt; public JwtAuthenticationFilter(JwtService jwt){this.jwt=jwt;}
 @Override protected void doFilterInternal(HttpServletRequest request,HttpServletResponse response,FilterChain chain)throws ServletException,IOException{
  String header=request.getHeader("Authorization");
  if(header!=null&&header.startsWith("Bearer ")){try{String username=jwt.validateAndGetUsername(header.substring(7));var authorities = "admin".equals(username) ? AuthorityUtils.createAuthorityList("ROLE_ADMIN") : AuthorityUtils.NO_AUTHORITIES;SecurityContextHolder.getContext().setAuthentication(new UsernamePasswordAuthenticationToken(username,null,authorities));}catch(RuntimeException ignored){SecurityContextHolder.clearContext();}}
  chain.doFilter(request,response);
 }
}
