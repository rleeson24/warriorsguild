SELECT i.emailaddress, u.email, c.firstname, c.lastname, 
(select count(*) from dbo.rankstatuses r where r.userid = c.id or r.userid = u.id) as rankActivity,
(select count(*) from dbo.ringstatuses r where r.userid = c.id or r.userid = u.id) as ringActivity,
(select count(*) from dbo.crossdayanswers r where r.userid = c.id or r.userid = u.id) as crossActivity
 FROM [dbo].[InvitedEmailAddresses] i
left outer join [dbo].[AspNetUsers] u
on i.emailaddress = u.email
left outer join [dbo].[aspnetusers] c
on c.applicationuserid = u.id
group by i.emailaddress, u.email, u.id, c.id, c.firstname, c.lastname