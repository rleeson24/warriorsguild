SELECT i.emailaddress, invitedat, case when u.Email IS NOT NULL then 'Yes' else 'No' end as HasRegistered FROM [dbo].[InvitedEmailAddresses] i
left outer join dbo.AspNetUsers u
on u.Email = i.EmailAddress;
select u.username, u.email, s.* from dbo.RankStatuses s
inner join dbo.AspNetUsers u
ON u.id = s.userId
where UserId IN (select u.id FROM [dbo].[InvitedEmailAddresses] i
left outer join dbo.AspNetUsers u
on u.Email = i.EmailAddress
where u.id is not null);

select u.username, u.email, s.*, r.* from dbo.RingStatuses s
inner join dbo.rings r
on r.id = s.ringId
inner join dbo.AspNetUsers u
ON u.id = s.userId
where UserId IN (select u.id FROM [dbo].[InvitedEmailAddresses] i
left outer join dbo.AspNetUsers u
on u.Email = i.EmailAddress
where u.id is not null);

select u.username, u.email, s.* from dbo.CrossDayStatuses s
inner join dbo.AspNetUsers u
ON u.id = s.userId
 where UserId IN (select u.id FROM [dbo].[InvitedEmailAddresses] i
left outer join dbo.AspNetUsers u
on u.Email = i.EmailAddress
where u.id is not null);