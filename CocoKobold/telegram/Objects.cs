using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoKobold.Telegram
{
    public class TGUpdate
    {
        public long update_id;
        public TGMessage message;
        public TGMessage edited_message;
        public TGMessage channel_post;
        public TGMessage edited_channel_post;
        public TGChatJoinRequest chat_join_request;
    }

    public class TGChatJoinRequest {
        public TGChat chat;
        public TGUser from;
        public int user_chat_id;
        public int date;
        public string bio;
        public TGChatInviteLink invite_link;
    }

    public class TGChatInviteLink
    {
        public string invite_link;
        public TGUser creator;
        public bool creates_join_request;
        public bool is_primary;
        public bool is_revoked;
        public string name;
        public int expire_date;
        public int member_limit;
        public int pending_join_request_count; 
    }

    public class TGUser
    {
        public long id;
        public bool is_bot;
        public string first_name;
        public string last_name;
        public string username;
        public string language_code;
        public bool is_premium;
        public bool added_to_attachment_menu;
        public bool can_join_groups;
        public bool can_read_all_group_messages;
        public bool supports_inline_requests;

    }

    public class TGChatPermissions
    {
        public bool can_send_messages;
        public bool can_send_audios;
        public bool can_send_documents;
        public bool can_send_photos;
        public bool can_send_videos;
        public bool can_send_video_notes;
        public bool can_send_voice_notes;
        public bool can_send_polls;
        public bool can_send_other_messages;
        public bool can_add_web_page_previews;
        public bool can_change_info;
        public bool can_invite_users;
        public bool can_pin_messages;
        public bool can_manage_topics;
    }

    public class TGChatAministratorRights
    {
        public bool can_manage_chat;
        public bool can_delete_messages;
        public bool can_manage_video_chats;
        public bool can_restrict_members;
        public bool can_promote_members;
    }

    public class TGChat
    {
        public long id;
        public string type;
        public string title;
        public string username;
        public string firstname;
        public string lastname;
        public bool all_members_are_administrators;
        public object photo; //!!
        public string description;
        public string invite_link;
        public object pinned_message; //!!
        public string sticker_set_name;
        public string can_set_sticker_set;
        public string[] active_usernames;
        public string bio;
        public bool has_private_forwards;
        public bool has_restricted_voice_and_video_messages;
        public bool join_to_send_messages;
        public bool join_by_request;
        public TGChatPermissions permissions;
        public int slow_mode_delay;
        public int message_auto_delete_time;
        public bool has_aggressive_anti_spam_enabled;
        public bool has_hidden_members;
        public bool has_protected_content;
        public int linked_chat_id;
       
    }


    public class TGPhotoSize
    {
        public string file_id;
        public int width;
        public int height;
        public int file_size;
    }
    public class TGVideo
    {
        public string file_id;
        public string file_unique_id;
    }
    public class TGVideoNote
    {
        public string file_id;
        public string file_unique_id;
    }
    public class TGDocument
    {
        public string file_id;
        public string file_unique_id;
    }

    public class TGMessageEntity
    {
        public string type;
        public int offset;
        public int length;
        public string url;
        public TGUser user;
        public string language;
        public string custom_emoji_id;
    }

    public class TGMessage
    {
        public long message_id;
        public TGUser from;
        public int date;
        public TGChat chat;
        public TGUser forward_from;
        public TGChat forward_from_chat;
        public int forward_from_message_id;
        public string forward_signature;
        public string forward_sender_name;
        public int forward_date;
        public bool is_topic_message;
        public bool is_automatic_forward;
        public TGMessage reply_to_message;
        public bool via_bot;
        public int edit_date;
        public bool has_protected_content;
        public string media_group_id;
        public string author_signature;
        public string text;
        public TGMessageEntity[] entities;
        public TGPhotoSize[] photo;
        public TGUser[] new_chat_members;
        public TGVideo video;
        public TGDocument document;
        public TGVideoNote video_note;
        public string caption;
        public bool has_media_spoiler;
    }
}
