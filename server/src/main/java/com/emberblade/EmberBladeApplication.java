package com.emberblade;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.scheduling.annotation.EnableScheduling;

@SpringBootApplication
@EnableScheduling
public class EmberBladeApplication {
    public static void main(String[] args) {
        SpringApplication.run(EmberBladeApplication.class, args);
    }
}
