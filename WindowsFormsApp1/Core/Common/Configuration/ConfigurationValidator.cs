using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WindowsFormsApp1.Core.Common.Configuration
{
    public static class ConfigurationValidator
    {
        public static bool ValidateSettings(AppSettings settings, out List<string> errors)
        {
            errors = new List<string>();
            
            // Validate PLC settings
            if (string.IsNullOrWhiteSpace(settings.Plc.SerialPort))
            {
                errors.Add("PLC serial port cannot be empty");
            }
            
            if (settings.Plc.BaudRate <= 0)
            {
                errors.Add("PLC baud rate must be greater than 0");
            }
            
            // Validate Camera settings
            // Camera device can be empty if not configured
            
            // Validate gRPC settings
            if (string.IsNullOrWhiteSpace(settings.Grpc.Endpoint))
            {
                errors.Add("gRPC endpoint cannot be empty");
            }
            
            if (settings.Grpc.MaxSendMessageLength <= 0)
            {
                errors.Add("gRPC max send message length must be greater than 0");
            }
            
            if (settings.Grpc.MaxReceiveMessageLength <= 0)
            {
                errors.Add("gRPC max receive message length must be greater than 0");
            }
            
            // Validate Database settings
            if (string.IsNullOrWhiteSpace(settings.Database.ConnectionString))
            {
                errors.Add("Database connection string cannot be empty");
            }
            
            return errors.Count == 0;
        }
        
        public static bool ValidatePlcSettings(PlcSettings settings, out List<string> errors)
        {
            errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(settings.SerialPort))
            {
                errors.Add("PLC serial port cannot be empty");
            }
            
            if (settings.BaudRate <= 0)
            {
                errors.Add("PLC baud rate must be greater than 0");
            }
            
            return errors.Count == 0;
        }
        
        public static bool ValidateGrpcSettings(GrpcSettings settings, out List<string> errors)
        {
            errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(settings.Endpoint))
            {
                errors.Add("gRPC endpoint cannot be empty");
            }
            
            if (settings.MaxSendMessageLength <= 0)
            {
                errors.Add("gRPC max send message length must be greater than 0");
            }
            
            if (settings.MaxReceiveMessageLength <= 0)
            {
                errors.Add("gRPC max receive message length must be greater than 0");
            }
            
            return errors.Count == 0;
        }
    }
}