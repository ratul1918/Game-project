package com.thelighthouse.server.web;
import com.thelighthouse.server.save.SaveConflictException; import java.util.Map;
import org.springframework.http.*; import org.springframework.web.bind.MethodArgumentNotValidException; import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
@RestControllerAdvice public class ApiExceptionHandler {
 @ExceptionHandler(SaveConflictException.class) ResponseEntity<?> conflict(SaveConflictException e){var body=new java.util.LinkedHashMap<String,Object>();body.put("error","SAVE_CONFLICT");body.put("serverCopy",e.getServerCopy());return ResponseEntity.status(HttpStatus.CONFLICT).body(body);}
 @ExceptionHandler(MethodArgumentNotValidException.class) ResponseEntity<?> invalid(){return ResponseEntity.badRequest().body(Map.of("error","INVALID_REQUEST","message","Request validation failed"));}
 @ExceptionHandler(ResponseStatusException.class) ResponseEntity<?> status(ResponseStatusException e){return ResponseEntity.status(e.getStatusCode()).body(Map.of("error",e.getStatusCode().toString(),"message",e.getReason()));}
}
