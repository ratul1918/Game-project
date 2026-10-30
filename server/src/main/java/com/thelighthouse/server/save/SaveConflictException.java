package com.thelighthouse.server.save;

public class SaveConflictException extends RuntimeException {
    private final SaveResponse serverCopy;
    public SaveConflictException(SaveResponse serverCopy){this.serverCopy=serverCopy;}
    public SaveResponse getServerCopy(){return serverCopy;}
}
