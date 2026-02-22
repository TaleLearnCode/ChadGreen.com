const { app } = require('@azure/functions');
const { EmailClient } = require('@azure/communication-email');

// Inquiry type configurations with required fields and email templates
const inquiryTypes = {
    general: {
        name: 'General Inquiry',
        requiredFields: ['name', 'email', 'subject', 'message'],
        subject: (data) => `General Inquiry: ${data.subject}`
    },
    speaking: {
        name: 'Speaking Engagement Inquiry',
        requiredFields: ['name', 'email', 'organization', 'eventName', 'eventDate', 'location', 'message'],
        optionalFields: ['audienceSize', 'eventType', 'preferredTopics', 'budgetRange'],
        subject: (data) => `Speaking Inquiry: ${data.eventName} - ${data.organization}`
    },
    media: {
        name: 'Media/Interview Request',
        requiredFields: ['name', 'email', 'outlet', 'interviewType', 'message'],
        optionalFields: ['deadline', 'topicsOfInterest'],
        subject: (data) => `Media Request: ${data.outlet} - ${data.interviewType}`
    },
    collaboration: {
        name: 'Collaboration/Partnership Inquiry',
        requiredFields: ['name', 'email', 'organization', 'partnershipType', 'projectDescription', 'message'],
        optionalFields: ['timeline'],
        subject: (data) => `Partnership Inquiry: ${data.partnershipType} - ${data.organization}`
    },
    feedback: {
        name: 'Feedback/Testimonial',
        requiredFields: ['name', 'email', 'eventAttended', 'testimonial'],
        optionalFields: ['rating', 'permissionToPublish'],
        subject: (data) => `Feedback: ${data.eventAttended}`
    }
};

// Email validation regex
const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

// Sanitize input to prevent injection
function sanitize(str) {
    if (typeof str !== 'string') return str;
    return str
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#x27;');
}

// Validate required fields
function validateFields(data, inquiryConfig) {
    const errors = [];
    
    for (const field of inquiryConfig.requiredFields) {
        if (!data[field] || (typeof data[field] === 'string' && !data[field].trim())) {
            errors.push(`${field} is required`);
        }
    }
    
    if (data.email && !emailRegex.test(data.email)) {
        errors.push('Invalid email address');
    }
    
    return errors;
}

// Format email body based on inquiry type
function formatEmailBody(type, data, inquiryConfig) {
    const allFields = [...inquiryConfig.requiredFields, ...(inquiryConfig.optionalFields || [])];
    
    let body = `<h2>${inquiryConfig.name}</h2>`;
    body += `<p>Received: ${new Date().toLocaleString()}</p>`;
    body += '<hr>';
    body += '<table style="border-collapse: collapse; width: 100%;">';
    
    for (const field of allFields) {
        if (data[field]) {
            const label = field.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase());
            const value = sanitize(data[field]);
            body += `<tr style="border-bottom: 1px solid #eee;">
                <td style="padding: 8px; font-weight: bold; width: 200px;">${label}</td>
                <td style="padding: 8px;">${value}</td>
            </tr>`;
        }
    }
    
    body += '</table>';
    
    return body;
}

app.http('contact', {
    methods: ['POST'],
    authLevel: 'anonymous',
    handler: async (request, context) => {
        context.log('Contact form submission received');
        
        try {
            const data = await request.json();
            
            // Honeypot check - if this field has a value, it's likely spam
            if (data.website) {
                context.log('Honeypot triggered - likely spam');
                // Return success to not alert spammers
                return {
                    status: 200,
                    jsonBody: { success: true }
                };
            }
            
            // Validate inquiry type
            const inquiryType = data.inquiryType;
            if (!inquiryType || !inquiryTypes[inquiryType]) {
                return {
                    status: 400,
                    jsonBody: { 
                        success: false, 
                        error: 'Invalid inquiry type' 
                    }
                };
            }
            
            const inquiryConfig = inquiryTypes[inquiryType];
            
            // Validate required fields
            const validationErrors = validateFields(data, inquiryConfig);
            if (validationErrors.length > 0) {
                return {
                    status: 400,
                    jsonBody: { 
                        success: false, 
                        errors: validationErrors 
                    }
                };
            }
            
            // Send email via Azure Communication Services
            const connectionString = process.env.ACS_CONNECTION_STRING;
            const senderEmail = process.env.SENDER_EMAIL;
            const recipientEmail = process.env.RECIPIENT_EMAIL;
            
            if (!connectionString || connectionString === 'your-azure-communication-services-connection-string') {
                context.log('ACS not configured - logging form submission');
                context.log('Form data:', JSON.stringify(data, null, 2));
                
                // In development, just log and return success
                return {
                    status: 200,
                    jsonBody: { 
                        success: true, 
                        message: 'Form submitted successfully (development mode)' 
                    }
                };
            }
            
            const emailClient = new EmailClient(connectionString);
            
            const emailMessage = {
                senderAddress: senderEmail,
                recipients: {
                    to: [{ address: recipientEmail }]
                },
                content: {
                    subject: inquiryConfig.subject(data),
                    html: formatEmailBody(inquiryType, data, inquiryConfig)
                },
                replyTo: [{ address: data.email, displayName: data.name }]
            };
            
            const poller = await emailClient.beginSend(emailMessage);
            const result = await poller.pollUntilDone();
            
            context.log('Email sent successfully:', result.id);
            
            return {
                status: 200,
                jsonBody: { 
                    success: true, 
                    message: 'Thank you for your message. I will get back to you soon!' 
                }
            };
            
        } catch (error) {
            context.error('Error processing contact form:', error);
            
            return {
                status: 500,
                jsonBody: { 
                    success: false, 
                    error: 'An error occurred while processing your request. Please try again later.' 
                }
            };
        }
    }
});
