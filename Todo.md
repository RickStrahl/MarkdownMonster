### Bugs and Needed Features
* [ ] Open as Diff breaks when opening an Untitled document
* [x] Tweak VS Code Dark Color Theme
* [ ] Check into Markdown nested coloring issues in Markdown syntax files
* [ ] WordPress oAuth Vulnerability (oAuth Token)

```
WeblogAddin.dll embeds the confidential WordPress.com OAuth client
credentials as compiled constants, and ObtainWordPressComToken() POSTs
them to WordPress.com's own token endpoint:
- client_id 132160, client_secret Pcl6sR…c7BoN
- POST to https://public-api.wordpress.com/oauth2/token with
grant_type=password (the Resource Owner Password flow), which requires
the confidential client_secret — there is no PKCE.

Because the secret ships in the freely downloadable app, anyone can
extract it. I've masked it here to avoid putting a live credential in
email; I have the full value and the file:line and can send them by
whatever channel you prefer.
```

* [ ] File location tracking is not working
* [ ] TOC generation has wrong spacing if there items start with header `###`
* [ ] Gist Addin - Fix Refresh in Previewer* 
* [ ] Table Editor - New Table with Column Size Selection
      possibly a column header editor that lets you create columns 
      and headings

### Consideration
* [ ] Markup (Publishing) support
* [ ] reveal.js presentations as an Addin
* [ ] FTP Upload Addin (started)
